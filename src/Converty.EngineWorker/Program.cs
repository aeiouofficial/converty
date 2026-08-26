using Converty.Contracts.Identifiers;
using Converty.Core.Presets;
using Converty.Provider.FFmpeg;

namespace Converty.EngineWorker;

internal static class Program
{
    private const int InvalidRequest = 2;
    private const int MissingEngineOrInput = 3;
    private const int AccessFailure = 5;
    private const int TimedOut = 6;
    private const int UnexpectedFailure = 10;

    public static async Task<int> Main(string[] args)
    {
        try
        {
            WorkerRequest request = Parse(args);
            ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(request.PresetId);
            ValidateStagingPaths(request, preset);

            string ffmpegPath = TrustedFfmpegPath.ResolveFromApplicationBaseDirectory();
            FfmpegExecutionResult result = await FfmpegProcessLauncher.ExecuteAsync(
                ffmpegPath,
                preset,
                request.InputPath,
                request.OutputPath,
                FfmpegProcessLauncher.MaximumExecutionTimeout).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                await Console.Error.WriteAsync(result.StandardError).ConfigureAwait(false);
            }

            return result.ExitCode;
        }
        catch (Exception error) when (error is ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            await Console.Error.WriteAsync("Invalid worker conversion request.").ConfigureAwait(false);
            return InvalidRequest;
        }
        catch (FileNotFoundException)
        {
            await Console.Error.WriteAsync("Required staged input or bundled conversion engine is missing.").ConfigureAwait(false);
            return MissingEngineOrInput;
        }
        catch (DirectoryNotFoundException)
        {
            await Console.Error.WriteAsync("Required worker directory is missing.").ConfigureAwait(false);
            return MissingEngineOrInput;
        }
        catch (UnauthorizedAccessException)
        {
            await Console.Error.WriteAsync("Worker access was denied.").ConfigureAwait(false);
            return AccessFailure;
        }
        catch (TimeoutException)
        {
            await Console.Error.WriteAsync("Conversion engine exceeded its execution timeout.").ConfigureAwait(false);
            return TimedOut;
        }
#pragma warning disable CA1031 // Final disposable-process boundary; caller receives only a bounded exit status.
        catch (Exception)
        {
            await Console.Error.WriteAsync("Unexpected conversion worker failure.").ConfigureAwait(false);
            return UnexpectedFailure;
        }
#pragma warning restore CA1031
    }

    private static WorkerRequest Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length != 6 ||
            !string.Equals(args[0], "--preset", StringComparison.Ordinal) ||
            !string.Equals(args[2], "--input", StringComparison.Ordinal) ||
            !string.Equals(args[4], "--output", StringComparison.Ordinal))
        {
            throw new ArgumentException("Worker accepts only the fixed preset/input/output argument surface.", nameof(args));
        }

        return new WorkerRequest(
            PresetId.Parse(args[1]),
            Path.GetFullPath(args[3]),
            Path.GetFullPath(args[5]));
    }

    private static void ValidateStagingPaths(WorkerRequest request, ProductPresetDefinition preset)
    {
        if (!Path.IsPathFullyQualified(request.InputPath) || !Path.IsPathFullyQualified(request.OutputPath))
        {
            throw new ArgumentException("Worker paths must be fully qualified.");
        }

        if (!File.Exists(request.InputPath))
        {
            throw new FileNotFoundException("Staged input does not exist.", request.InputPath);
        }

        if (File.Exists(request.OutputPath) || Directory.Exists(request.OutputPath))
        {
            throw new IOException("Staged output path must not already exist.");
        }

        string? inputDirectory = Path.GetDirectoryName(request.InputPath);
        string? outputDirectory = Path.GetDirectoryName(request.OutputPath);
        if (string.IsNullOrWhiteSpace(inputDirectory) ||
            string.IsNullOrWhiteSpace(outputDirectory) ||
            !string.Equals(inputDirectory, outputDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Worker input and output must remain within one private staging directory.");
        }

        if (!preset.SupportsPath(request.InputPath) ||
            !string.Equals(Path.GetExtension(request.OutputPath), preset.OutputExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Staged paths do not match the selected preset.");
        }
    }

    private sealed record WorkerRequest(PresetId PresetId, string InputPath, string OutputPath);
}
