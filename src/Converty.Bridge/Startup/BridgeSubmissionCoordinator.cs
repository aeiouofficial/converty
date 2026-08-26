using Converty.Bridge.Ipc;
using Converty.Contracts.Conversion;

namespace Converty.Bridge.Startup;

public sealed class BridgeSubmissionCoordinator
{
    public static readonly TimeSpan MaximumStartupTimeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan MaximumRetryDelay = TimeSpan.FromSeconds(1);

    private readonly IBridgeRequestClient _client;
    private readonly IHostProcessLauncher _launcher;
    private readonly TimeSpan _startupTimeout;
    private readonly TimeSpan _retryDelay;

    public BridgeSubmissionCoordinator(
        IBridgeRequestClient client,
        IHostProcessLauncher launcher,
        TimeSpan startupTimeout,
        TimeSpan retryDelay)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));

        if (startupTimeout <= TimeSpan.Zero || startupTimeout > MaximumStartupTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startupTimeout),
                $"Startup timeout must be greater than zero and at most {MaximumStartupTimeout.TotalSeconds} seconds.");
        }

        if (retryDelay <= TimeSpan.Zero || retryDelay > MaximumRetryDelay || retryDelay >= startupTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retryDelay),
                "Retry delay must be greater than zero, no more than one second, and shorter than the startup timeout.");
        }

        _startupTimeout = startupTimeout;
        _retryDelay = retryDelay;
    }

    public async Task<BridgeSubmissionResult> SubmitAsync(
        ConversionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await _client.SubmitAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (BridgeHostUnavailableException)
        {
            // Only a connect-stage unavailable Host is eligible for trusted startup.
        }

        cancellationToken.ThrowIfCancellationRequested();
        _launcher.StartHost();

        using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        startupCancellation.CancelAfter(_startupTimeout);

        try
        {
            while (true)
            {
                await Task.Delay(_retryDelay, startupCancellation.Token).ConfigureAwait(false);
                try
                {
                    return await _client.SubmitAsync(request, startupCancellation.Token).ConfigureAwait(false);
                }
                catch (BridgeHostUnavailableException)
                {
                    // Retry until the single bounded startup deadline expires.
                }
            }
        }
        catch (OperationCanceledException error) when (
            !cancellationToken.IsCancellationRequested && startupCancellation.IsCancellationRequested)
        {
            throw new TimeoutException(
                "Trusted Converty Host did not become available within the startup deadline.",
                error);
        }
    }
}
