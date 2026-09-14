using System.Security.Cryptography;
using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Output;
using Converty.Core.Planning;
using Converty.Core.Presets;

namespace Converty.Core.Execution;

public sealed class ConversionBatchRunner
{
    private const int MaximumPublishRaceRetries = 64;
    public static readonly TimeSpan MaximumExecutionTimeout = TimeSpan.FromMinutes(30);

    private readonly ProductPresetRegistry _presets;
    private readonly OutputPathResolver _outputPaths;
    private readonly IConversionWorkerClient _workerClient;
    private readonly IMediaProbeClient? _probeClient;
    private readonly ConversionPlanner? _videoPlanner;
    private readonly TimeSpan _executionTimeout;

    public ConversionBatchRunner(
        ProductPresetRegistry presets,
        OutputPathResolver outputPaths,
        IConversionWorkerClient workerClient,
        TimeSpan executionTimeout)
    {
        _presets = presets ?? throw new ArgumentNullException(nameof(presets));
        _outputPaths = outputPaths ?? throw new ArgumentNullException(nameof(outputPaths));
        _workerClient = workerClient ?? throw new ArgumentNullException(nameof(workerClient));
        ValidateExecutionTimeout(executionTimeout);
        _executionTimeout = executionTimeout;
    }

    public ConversionBatchRunner(
        ProductPresetRegistry presets,
        OutputPathResolver outputPaths,
        IConversionWorkerClient workerClient,
        IMediaProbeClient probeClient,
        ConversionPlanner videoPlanner,
        TimeSpan executionTimeout)
        : this(presets, outputPaths, workerClient, executionTimeout)
    {
        _probeClient = probeClient ?? throw new ArgumentNullException(nameof(probeClient));
        _videoPlanner = videoPlanner ?? throw new ArgumentNullException(nameof(videoPlanner));
    }

    public async Task<ConversionBatchResult> RunAsync(
        PresetId presetId,
        IReadOnlyList<string> inputPaths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presetId);
        ArgumentNullException.ThrowIfNull(inputPaths);
        if (inputPaths.Count is < 1 or > ConversionRequest.MaximumFiles)
        {
            throw new ArgumentException(
                $"Conversion batch must contain 1-{ConversionRequest.MaximumFiles} files.",
                nameof(inputPaths));
        }

        ProductPresetDefinition preset = _presets.GetRequired(presetId);
        var results = new List<ConversionFileResult>(inputPaths.Count);
        ConversionFailedException? firstConversionFailure = null;

        foreach (string inputPath in inputPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateInputPath(inputPath, preset);

            string plannedOutputPath = _outputPaths.Resolve(inputPath, preset.OutputExtension);
            ConversionStagingPaths staging = ConversionStagingDirectory.Create(inputPath, preset.OutputExtension);
            try
            {
                try
                {
                    ConversionWorkerResult execution;
                    if (preset.InputKind == ProductMediaKind.Video)
                    {
                        execution = await ExecuteVideoAsync(
                            inputPath,
                            plannedOutputPath,
                            preset,
                            staging,
                            cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        execution = await _workerClient.ExecuteAsync(
                            preset.Id,
                            staging.InputPath,
                            staging.OutputPath,
                            _executionTimeout,
                            cancellationToken).ConfigureAwait(false);
                        EnsureSuccessfulWorkerOutput(inputPath, plannedOutputPath, staging.OutputPath, execution);
                    }

                    string publishedOutputPath = PublishTemporaryOutput(
                        inputPath,
                        preset.OutputExtension,
                        staging.OutputPath);
                    results.Add(new ConversionFileResult(inputPath, publishedOutputPath, execution.ExitCode));
                }
                catch (ConversionFailedException error)
                {
                    // One bad selection must not suppress later independent files in the same Explorer batch.
                    firstConversionFailure ??= error;
                }
            }
            finally
            {
                ConversionStagingDirectory.DeleteOwned(staging.DirectoryPath);
            }
        }

        if (firstConversionFailure is not null)
        {
            throw firstConversionFailure;
        }

        return new ConversionBatchResult(results.AsReadOnly());
    }

    private async Task<ConversionWorkerResult> ExecuteVideoAsync(
        string inputPath,
        string plannedOutputPath,
        ProductPresetDefinition preset,
        ConversionStagingPaths staging,
        CancellationToken cancellationToken)
    {
        if (_probeClient is null || _videoPlanner is null)
        {
            throw new ConversionFailedException(
                inputPath,
                plannedOutputPath,
                exitCode: null,
                "Video conversion requires the qualified probe and planning pipeline.");
        }

        MediaProbeResultV1 inputProbe = await _probeClient.ProbeAsync(
            staging.InputPath,
            _executionTimeout,
            cancellationToken).ConfigureAwait(false);
        MediaProbeFactsV1 inputFacts = RequireSuccessfulProbe(
            inputPath,
            plannedOutputPath,
            inputProbe,
            "Video input probe did not produce qualified media facts.");

        ConversionPlan plan;
        TargetMediaContract targetContract;
        try
        {
            FormatId sourceFormat = VideoProductCapabilityCatalog.ResolveSourceFormat(inputFacts.Container);
            FormatId targetFormat = VideoProductCapabilityCatalog.ResolveTargetFormat(preset.Id);
            var source = new ProbedFileDescriptor(
                staging.InputPath,
                VideoProductCapabilityCatalog.VideoFamilyId,
                sourceFormat,
                new FileInfo(staging.InputPath).Length,
                inputFacts);
            plan = _videoPlanner.Plan(new PlanningRequest(
                Guid.NewGuid(),
                source,
                targetFormat,
                VideoProductCapabilityCatalog.EngineProviderId,
                preset.Id,
                allowIdentity: true));
            targetContract = TargetMediaContract.ForPlan(preset.Id, plan.Mode, inputFacts);
        }
        catch (InvalidOperationException)
        {
            throw new ConversionFailedException(
                inputPath,
                plannedOutputPath,
                exitCode: null,
                "Video input is not supported by the qualified planning policy.");
        }

        ConversionWorkerResult execution = await _workerClient.ExecuteAsync(
            preset.Id,
            plan.Mode,
            staging.InputPath,
            staging.OutputPath,
            _executionTimeout,
            cancellationToken).ConfigureAwait(false);
        EnsureSuccessfulWorkerOutput(inputPath, plannedOutputPath, staging.OutputPath, execution);

        if (plan.Mode == ConversionMode.Copy)
        {
            await VerifyCopySha256Async(
                inputPath,
                plannedOutputPath,
                staging.InputPath,
                staging.OutputPath,
                execution.ExitCode,
                cancellationToken).ConfigureAwait(false);
        }

        MediaProbeResultV1 outputProbe = await _probeClient.ProbeAsync(
            staging.OutputPath,
            _executionTimeout,
            cancellationToken).ConfigureAwait(false);
        MediaProbeFactsV1 outputFacts = RequireSuccessfulProbe(
            inputPath,
            plannedOutputPath,
            outputProbe,
            "Staged Video output could not be post-probed.",
            execution.ExitCode);

        try
        {
            TargetMediaContractValidator.Validate(targetContract, outputFacts);
        }
        catch (InvalidOperationException)
        {
            throw new ConversionFailedException(
                inputPath,
                plannedOutputPath,
                execution.ExitCode,
                "Staged Video output did not satisfy the selected target contract.");
        }

        return execution;
    }

    private static MediaProbeFactsV1 RequireSuccessfulProbe(
        string inputPath,
        string plannedOutputPath,
        MediaProbeResultV1 result,
        string message,
        int? exitCode = null)
    {
        if (result.Status != MediaProbeStatus.Success || result.Facts is null)
        {
            throw new ConversionFailedException(inputPath, plannedOutputPath, exitCode, message);
        }

        return result.Facts;
    }

    private static void EnsureSuccessfulWorkerOutput(
        string inputPath,
        string plannedOutputPath,
        string stagedOutputPath,
        ConversionWorkerResult execution)
    {
        if (!execution.Succeeded)
        {
            string detail = string.IsNullOrWhiteSpace(execution.StandardError)
                ? "Conversion worker reported a failure."
                : $"Conversion worker reported a failure: {execution.StandardError}";
            throw new ConversionFailedException(inputPath, plannedOutputPath, execution.ExitCode, detail);
        }

        if (!File.Exists(stagedOutputPath) || new FileInfo(stagedOutputPath).Length == 0)
        {
            throw new ConversionFailedException(
                inputPath,
                plannedOutputPath,
                execution.ExitCode,
                "Conversion worker exited successfully but did not produce a non-empty output file.");
        }
    }

    private static async Task VerifyCopySha256Async(
        string inputPath,
        string plannedOutputPath,
        string stagedInputPath,
        string stagedOutputPath,
        int exitCode,
        CancellationToken cancellationToken)
    {
        byte[] inputHash = await ComputeSha256Async(stagedInputPath, cancellationToken).ConfigureAwait(false);
        byte[] outputHash = await ComputeSha256Async(stagedOutputPath, cancellationToken).ConfigureAwait(false);
        if (!CryptographicOperations.FixedTimeEquals(inputHash, outputHash))
        {
            throw new ConversionFailedException(
                inputPath,
                plannedOutputPath,
                exitCode,
                "Managed Copy output failed independent SHA-256 equality verification.");
        }
    }

    private static async Task<byte[]> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            128 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    private string PublishTemporaryOutput(
        string inputPath,
        string outputExtension,
        string temporaryOutputPath)
    {
        for (int attempt = 0; attempt < MaximumPublishRaceRetries; ++attempt)
        {
            string outputPath = _outputPaths.Resolve(inputPath, outputExtension);
            try
            {
                File.Move(temporaryOutputPath, outputPath, overwrite: false);
                return outputPath;
            }
            catch (IOException) when (File.Exists(outputPath) || Directory.Exists(outputPath))
            {
                // Another writer won the destination race. Resolve the next numbered path; never overwrite it.
            }
        }

        throw new IOException(
            $"Unable to publish converted output after {MaximumPublishRaceRetries} destination races.");
    }

    private static void ValidateInputPath(string inputPath, ProductPresetDefinition preset)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || inputPath.Length > ConversionRequest.MaximumPathLength)
        {
            throw new ArgumentException("Input path is missing or exceeds the Windows path limit.", nameof(inputPath));
        }

        if (!Path.IsPathFullyQualified(inputPath))
        {
            throw new ArgumentException("Explorer input paths must be fully qualified.", nameof(inputPath));
        }

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Selected input file no longer exists.", inputPath);
        }

        if (!preset.SupportsPath(inputPath))
        {
            throw new InvalidOperationException(
                $"Preset '{preset.Id}' does not support input extension '{Path.GetExtension(inputPath)}'.");
        }
    }

    private static void ValidateExecutionTimeout(TimeSpan executionTimeout)
    {
        if (executionTimeout <= TimeSpan.Zero || executionTimeout > MaximumExecutionTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(executionTimeout));
        }
    }
}
