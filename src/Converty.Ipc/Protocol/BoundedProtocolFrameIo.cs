namespace Converty.Ipc.Protocol;

public static class BoundedProtocolFrameIo
{
    private static readonly TimeSpan MaximumTimeout = TimeSpan.FromSeconds(30);

    public static async ValueTask<ProtocolFrame> ReadAsync(
        Stream stream,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ValidateTimeout(timeout);
        cancellationToken.ThrowIfCancellationRequested();

        using CancellationTokenSource timeoutCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            return await ProtocolFrameCodec.ReadAsync(stream, timeoutCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("IPC frame read exceeded the configured timeout.");
        }
    }

    public static async ValueTask WriteAndFlushAsync(
        Stream stream,
        ReadOnlyMemory<byte> payload,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ValidateTimeout(timeout);
        cancellationToken.ThrowIfCancellationRequested();

        using CancellationTokenSource timeoutCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            await ProtocolFrameCodec.WriteAsync(stream, payload, timeoutCancellation.Token).ConfigureAwait(false);
            await stream.FlushAsync(timeoutCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("IPC frame write exceeded the configured timeout.");
        }
    }

    private static void ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout > MaximumTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                $"IPC timeout must be greater than zero and no more than {MaximumTimeout.TotalSeconds:0} seconds.");
        }
    }
}
