using Converty.Ipc.Protocol;

namespace Converty.Ipc.Tests.Protocol;

public sealed class BoundedProtocolFrameIoTests
{
    [Fact]
    public async Task ReadTimeoutFailsClosed()
    {
        await using var stream = new BlockingStream();

        TimeoutException error = await Assert.ThrowsAsync<TimeoutException>(async () =>
            await BoundedProtocolFrameIo.ReadAsync(
                stream,
                TimeSpan.FromMilliseconds(50),
                TestContext.Current.CancellationToken));

        Assert.Contains("read", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WriteTimeoutFailsClosed()
    {
        await using var stream = new BlockingStream();

        TimeoutException error = await Assert.ThrowsAsync<TimeoutException>(async () =>
            await BoundedProtocolFrameIo.WriteAndFlushAsync(
                stream,
                new byte[] { 0x01 },
                TimeSpan.FromMilliseconds(50),
                TestContext.Current.CancellationToken));

        Assert.Contains("write", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallerCancellationIsNotReclassifiedAsTimeout()
    {
        await using var stream = new BlockingStream();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await BoundedProtocolFrameIo.ReadAsync(stream, TimeSpan.FromSeconds(1), cancellation.Token));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    public async Task TimeoutMustBePositiveAndBounded(int seconds)
    {
        await using var stream = new BlockingStream();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await BoundedProtocolFrameIo.ReadAsync(
                stream,
                TimeSpan.FromSeconds(seconds),
                TestContext.Current.CancellationToken));
    }

    private sealed class BlockingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            new(Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ContinueWith(
                static _ => 0,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default));

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            new(Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken));

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}
