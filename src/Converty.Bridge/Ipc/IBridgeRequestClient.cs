using Converty.Contracts.Conversion;

namespace Converty.Bridge.Ipc;

public interface IBridgeRequestClient
{
    Task<BridgeSubmissionResult> SubmitAsync(
        ConversionRequest request,
        CancellationToken cancellationToken = default);
}
