using System.Runtime.Versioning;
using Converty.Bridge.Diagnostics;
using Converty.Bridge.Shell;
using Converty.Bridge.Workers;
using Converty.Core.Execution;
using Converty.Core.Output;
using Converty.Core.Presets;

namespace Converty.Bridge;

[SupportedOSPlatform("windows")]
internal static class Program
{
    private const int Success = 0;
    private const int InvalidRequest = 2;
    private const int MissingInputOrEngine = 3;
    private const int ConversionFailure = 4;
    private const int AccessFailure = 5;
    private const int TimedOut = 6;
    private const int UnexpectedFailure = 10;

    public static async Task<int> Main(string[] args)
    {
        try
        {
            ShellConversionRequest request = ShellConversionRequestParser.Parse(args);
            BridgePackageIdentityDiagnostic.TryWriteEvidence(request.InputPaths);
            var runner = new ConversionBatchRunner(
                ProductPresetRegistry.Default,
                new OutputPathResolver(),
                EngineWorkerClient.CreateForApplicationBaseDirectory(),
                ConversionBatchRunner.MaximumExecutionTimeout);
            _ = await runner.RunAsync(request.PresetId, request.InputPaths).ConfigureAwait(false);
            return Success;
        }
        catch (Exception error) when (error is ArgumentException or KeyNotFoundException)
        {
            BridgeErrorDialog.Show("Converty received an invalid conversion request.");
            return InvalidRequest;
        }
        catch (FileNotFoundException)
        {
            BridgeErrorDialog.Show("A selected file or the bundled conversion engine is no longer available.");
            return MissingInputOrEngine;
        }
        catch (UnauthorizedAccessException)
        {
            BridgeErrorDialog.Show("Converty does not have permission to read the source or create the converted file.");
            return AccessFailure;
        }
        catch (TimeoutException)
        {
            BridgeErrorDialog.Show("The conversion exceeded the maximum allowed execution time.");
            return TimedOut;
        }
        catch (ConversionFailedException error)
        {
            string inputName = Path.GetFileName(error.InputPath);
            BridgeErrorDialog.Show($"Conversion failed for '{inputName}'. {error.Message}");
            return ConversionFailure;
        }
        catch (IOException error)
        {
            BridgeErrorDialog.Show($"Converty could not create the converted output. {error.Message}");
            return ConversionFailure;
        }
#pragma warning disable CA1031 // Last-resort process boundary so Explorer-triggered failures are not silent.
        catch (Exception)
        {
            BridgeErrorDialog.Show("Converty encountered an unexpected conversion failure.");
            return UnexpectedFailure;
        }
#pragma warning restore CA1031
    }
}
