using Converty.Contracts.Jobs;

namespace Converty.Bridge.Ipc;

public interface IBridgeJobControlClient
{
    Task<JobControlResponse> GetStatusAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<JobControlResponse> CancelAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
}
