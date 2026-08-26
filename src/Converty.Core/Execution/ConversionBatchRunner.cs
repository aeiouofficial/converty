using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Output;
using Converty.Core.Presets;

namespace Converty.Core.Execution;

public sealed class ConversionBatchRunner
{
    private const int MaximumPublishRaceRetries = 64;
    public static readonly TimeSpan MaximumExecutionTimeout = TimeSpan.FromMinutes(30);

    private readonly ProductPresetRegistry _presets;
    private readonly OutputPathResolver _outputPaths;
    private readonly IConversionWorkerClient _workerClient;
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
        if (executionTimeout <= TimeSpan.Zero || executionTimeout > MaximumExecutionTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(executionTimeout));
        }

        _executionTimeout = executionTimeout;
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

        foreach (string inputPath in inputPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateInputPath(inputPath, preset);

            string plannedOutputPath = _outputPaths.Resolve(inputPath, preset.OutputExtension);
            ConversionStagingPaths staging = ConversionStagingDirectory.Create(inputPath, preset.OutputExtension);
            try
            {
                ConversionWorkerResult execution = await _workerClient.ExecuteAsync(
                    preset.Id,
                    staging.InputPath,
                    staging.OutputPath,
                    _executionTimeout,
                    cancellationToken).ConfigureAwait(false);

                if (!execution.Succeeded)
                {
                    string detail = string.IsNullOrWhiteSpace(execution.StandardError)
                        ? "Conversion worker reported a failure."
                        : $"Conversion worker reported a failure: {execution.StandardError}";
                    throw new ConversionFailedException(inputPath, plannedOutputPath, execution.ExitCode, detail);
                }

                if (!File.Exists(staging.OutputPath) || new FileInfo(staging.OutputPath).Length == 0)
                {
                    throw new ConversionFailedException(
                        inputPath,
                        plannedOutputPath,
                        execution.ExitCode,
                        "Conversion worker exited successfully but did not produce a non-empty output file.");
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
