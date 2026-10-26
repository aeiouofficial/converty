using Converty.Contracts.Conversion;

namespace Converty.Core.Execution;

public interface IMediaProbeClient
{
    Task<MediaProbeResultV1> ProbeAsync(
        string stagedInputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
