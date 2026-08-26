using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Output;
using Converty.Core.Presets;

namespace Converty.Core.Execution;

public sealed class ConversionBatchRunner
{
    private const int MaximumPublishRaceRetries = 64;

    private readonly ProductPresetRegistry _presets;
    private readonly OutputPathResolver _outputPaths;
    private readonly IFfmpegProcessLauncher _launcher;
    private readonly string _ffmpegPath;
    private readonly TimeSpan _executionTimeout;

    public ConversionBatchRunner(
        ProductPresetRegistry presets,
        OutputPathResolver outputPaths,
        IFfmpegProcessLauncher launcher,
        string ffmpegPath,
        TimeSpan executionTimeout)
    {
        _presets = presets ?? throw new ArgumentNullException(nameof(presets));
        _outputPaths = outputPaths ?? throw new ArgumentNullException(nameof(outputPaths));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        if (string.IsNullOrWhiteSpace(ffmpegPath) || !Path.IsPathFullyQualified(ffmpegPath))
        {
            throw new ArgumentException("Trusted FFmpeg path must be fully qualified.", nameof(ffmpegPath));
        }

        if (executionTimeout <= TimeSpan.Zero || executionTimeout > FfmpegProcessLauncher.MaximumExecutionTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(executionTimeout));
        }

        _ffmpegPath = ffmpegPath;
        _executionTimeout = executionTimeout;
    }

    public static ConversionBatchRunner CreateForApplicationBaseDirectory() =>
        new(
            ProductPresetRegistry.Default,
            new OutputPathResolver(),
            new FfmpegProcessLauncher(),
            TrustedFfmpegPath.ResolveFromApplicationBaseDirectory(),
            FfmpegProcessLauncher.MaximumExecutionTimeout);

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

        foreach (string inputPath in inputPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateInputPath(inputPath, preset);

            string plannedOutputPath = _outputPaths.Resolve(inputPath, preset.OutputExtension);
            ConversionStagingPaths staging = ConversionStagingDirectory.Create(inputPath, preset.OutputExtension);
            try
            {
                FfmpegExecutionResult execution = await _launcher.ExecuteAsync(
                    _ffmpegPath,
                    preset,
                    staging.InputPath,
                    staging.OutputPath,
                    _executionTimeout,
                    cancellationToken).ConfigureAwait(false);

                if (!execution.Succeeded)
                {
                    string detail = string.IsNullOrWhiteSpace(execution.StandardError)
                        ? "FFmpeg reported a conversion failure."
                        : $"FFmpeg reported a conversion failure: {execution.StandardError}";
                    throw new ConversionFailedException(inputPath, plannedOutputPath, execution.ExitCode, detail);
                }

                if (!File.Exists(staging.OutputPath) || new FileInfo(staging.OutputPath).Length == 0)
                {
                    throw new ConversionFailedException(
                        inputPath,
                        plannedOutputPath,
                        execution.ExitCode,
                        "FFmpeg exited successfully but did not produce a non-empty output file.");
                }

                string publishedOutputPath = PublishTemporaryOutput(
                    inputPath,
                    preset.OutputExtension,
                    staging.OutputPath);
                results.Add(new ConversionFileResult(inputPath, publishedOutputPath, execution.ExitCode));
            }
            finally
            {
                ConversionStagingDirectory.DeleteOwned(staging.DirectoryPath);
            }
        }

        return new ConversionBatchResult(results.AsReadOnly());
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
                // Another writer won the destination race after resolution. Re-resolve to the
                // next numbered copy; never remove or overwrite the competing destination.
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
}
