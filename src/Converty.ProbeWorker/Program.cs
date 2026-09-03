using System.Text.Json;
using Converty.Contracts.Conversion;
using Converty.Provider.FFmpeg;
using Converty.Serialization;

namespace Converty.ProbeWorker;

internal static class Program
{
    private const int InvalidRequest = 2;
    private const int MissingEngineOrInput = 3;
    private const int AccessFailure = 5;
    private const int UnexpectedFailure = 10;
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(30);

    public static async Task<int> Main(string[] args)
    {
        try
        {
            string inputPath = Parse(args);
            string ffprobePath = TrustedFfprobePath.ResolveFromApplicationBaseDirectory();
            FfprobeExecutionResult execution = await FfprobeProcessLauncher.ExecuteAsync(
                ffprobePath,
                inputPath,
                ProbeTimeout).ConfigureAwait(false);

            MediaProbeResultV1 result;
            if (execution.ExitCode != 0)
            {
                result = MediaProbeResultV1.Failure(MediaProbeFailureReason.UnsupportedInput);
            }
            else
            {
                MediaProbeFactsV1 facts = FfprobeJsonAdapter.Parse(inputPath, execution.StandardOutput);
                result = MediaProbeResultV1.Success(facts);
            }

            await Console.Out.WriteAsync(ContractJson.Serialize(result)).ConfigureAwait(false);
            return 0;
        }
        catch (TimeoutException)
        {
            await WriteSemanticFailureAsync(MediaProbeFailureReason.Timeout).ConfigureAwait(false);
            return 0;
        }
        catch (FfprobeOutputLimitExceededException)
        {
            await WriteSemanticFailureAsync(MediaProbeFailureReason.OutputLimitExceeded).ConfigureAwait(false);
            return 0;
        }
        catch (JsonException)
        {
            await WriteSemanticFailureAsync(MediaProbeFailureReason.MalformedOutput).ConfigureAwait(false);
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException)
        {
            await Console.Error.WriteAsync("Invalid probe worker request.").ConfigureAwait(false);
            return InvalidRequest;
        }
        catch (FileNotFoundException)
        {
            await Console.Error.WriteAsync("Required staged input or bundled probe engine is missing.").ConfigureAwait(false);
            return MissingEngineOrInput;
        }
        catch (DirectoryNotFoundException)
        {
            await Console.Error.WriteAsync("Required probe worker directory is missing.").ConfigureAwait(false);
            return MissingEngineOrInput;
        }
        catch (UnauthorizedAccessException)
        {
            await Console.Error.WriteAsync("Probe worker access was denied.").ConfigureAwait(false);
            return AccessFailure;
        }
#pragma warning disable CA1031
        catch (Exception)
        {
            await Console.Error.WriteAsync("Unexpected probe worker failure.").ConfigureAwait(false);
            return UnexpectedFailure;
        }
#pragma warning restore CA1031
    }

    private static string Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length != 2 || !string.Equals(args[0], "--input", StringComparison.Ordinal))
        {
            throw new ArgumentException("ProbeWorker accepts only the fixed --input argument surface.", nameof(args));
        }

        string inputPath = Path.GetFullPath(args[1]);
        if (!Path.IsPathFullyQualified(inputPath))
        {
            throw new ArgumentException("Probe input must be fully qualified.", nameof(args));
        }
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("Staged probe input does not exist.", inputPath);
        }
        if ((File.GetAttributes(inputPath) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Staged probe input must not be a reparse point.");
        }
        return inputPath;
    }

    private static Task WriteSemanticFailureAsync(MediaProbeFailureReason reason) =>
        Console.Out.WriteAsync(ContractJson.Serialize(MediaProbeResultV1.Failure(reason)));
}
