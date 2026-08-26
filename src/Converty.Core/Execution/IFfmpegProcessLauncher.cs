using Converty.Core.Presets;

namespace Converty.Core.Execution;

public interface IFfmpegProcessLauncher
{
    Task<FfmpegExecutionResult> ExecuteAsync(
        string ffmpegPath,
        ProductPresetDefinition preset,
        string inputPath,
        string outputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
