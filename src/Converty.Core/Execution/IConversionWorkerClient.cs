using Converty.Contracts.Conversion;
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

    Task<ConversionWorkerResult> ExecuteAsync(
        PresetId presetId,
        ConversionMode mode,
        string stagedInputPath,
        string stagedOutputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("This conversion worker client does not support explicit conversion modes.");
}
