using Converty.Contracts.Identifiers;

namespace Converty.Core.Execution;

public interface IConversionWorkerClient
{
    Task<ConversionWorkerResult> ExecuteAsync(
        PresetId presetId,
        string stagedInputPath,
        string stagedOutputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
