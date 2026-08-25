using System.Buffers.Binary;
using Converty.Ipc.Protocol;

namespace Converty.Ipc.Tests.Protocol;

public sealed class FrameCodecTests
{
    [Fact]
    public async Task RoundTripPreservesVersionAndPayload()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] payload = [0x01, 0x02, 0x7F, 0xFF];
        await using var stream = new MemoryStream();

        await ProtocolFrameCodec.WriteAsync(stream, payload, token);
        stream.Position = 0;
        ProtocolFrame frame = await ProtocolFrameCodec.ReadAsync(stream, token);

        Assert.Equal(ProtocolLimits.CurrentVersion, frame.Version);
        Assert.Equal(payload, frame.Payload.ToArray());
    }

    [Fact]
    public async Task WriteRejectsPayloadOverConfiguredLimit()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] payload = new byte[ProtocolLimits.MaxPayloadBytes + 1];
        await using var stream = new MemoryStream();

        await Assert.ThrowsAsync<ProtocolException>(async () =>
            await ProtocolFrameCodec.WriteAsync(stream, payload, token));
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public async Task ReadRejectsBadMagicBeforePayloadAllocation()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] header = BuildHeader(0x01020304u, ProtocolLimits.CurrentVersion, 16);
        await using var stream = new MemoryStream(header);

        ProtocolException error = await Assert.ThrowsAsync<ProtocolException>(async () =>
            await ProtocolFrameCodec.ReadAsync(stream, token));

        Assert.Equal(ProtocolErrorCode.BadMagic, error.ErrorCode);
    }

    [Fact]
    public async Task ReadRejectsUnsupportedVersion()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] header = BuildHeader(ProtocolLimits.Magic, checked((ushort)(ProtocolLimits.CurrentVersion + 1)), 0);
        await using var stream = new MemoryStream(header);

        ProtocolException error = await Assert.ThrowsAsync<ProtocolException>(async () =>
            await ProtocolFrameCodec.ReadAsync(stream, token));

        Assert.Equal(ProtocolErrorCode.UnsupportedVersion, error.ErrorCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task ReadRejectsNegativePayloadLength(int payloadLength)
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] header = BuildHeader(ProtocolLimits.Magic, ProtocolLimits.CurrentVersion, payloadLength);
        await using var stream = new MemoryStream(header);

        ProtocolException error = await Assert.ThrowsAsync<ProtocolException>(async () =>
            await ProtocolFrameCodec.ReadAsync(stream, token));

        Assert.Equal(ProtocolErrorCode.InvalidLength, error.ErrorCode);
    }

    [Fact]
    public async Task ReadRejectsOversizedPayloadLengthBeforeReadingPayload()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] header = BuildHeader(ProtocolLimits.Magic, ProtocolLimits.CurrentVersion, ProtocolLimits.MaxPayloadBytes + 1);
        await using var stream = new MemoryStream(header);

        ProtocolException error = await Assert.ThrowsAsync<ProtocolException>(async () =>
            await ProtocolFrameCodec.ReadAsync(stream, token));

        Assert.Equal(ProtocolErrorCode.FrameTooLarge, error.ErrorCode);
        Assert.Equal(ProtocolLimits.HeaderSize, stream.Position);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(11)]
    public async Task ReadRejectsTruncatedHeader(int availableBytes)
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] bytes = new byte[availableBytes];
        await using var stream = new MemoryStream(bytes);

        ProtocolException error = await Assert.ThrowsAsync<ProtocolException>(async () =>
            await ProtocolFrameCodec.ReadAsync(stream, token));

        Assert.Equal(ProtocolErrorCode.TruncatedFrame, error.ErrorCode);
    }

    [Fact]
    public async Task ReadRejectsTruncatedPayload()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        byte[] header = BuildHeader(ProtocolLimits.Magic, ProtocolLimits.CurrentVersion, 4);
        byte[] bytes = [.. header, 0x10, 0x20];
        await using var stream = new MemoryStream(bytes);

        ProtocolException error = await Assert.ThrowsAsync<ProtocolException>(async () =>
            await ProtocolFrameCodec.ReadAsync(stream, token));

        Assert.Equal(ProtocolErrorCode.TruncatedFrame, error.ErrorCode);
    }

    [Fact]
    public async Task CancelledWriteDoesNotEmitFrame()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        await using var stream = new MemoryStream();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await ProtocolFrameCodec.WriteAsync(stream, new byte[] { 0x01 }, cancellation.Token));
        Assert.Equal(0, stream.Length);
    }

    private static byte[] BuildHeader(uint magic, ushort version, int payloadLength)
    {
        byte[] header = new byte[ProtocolLimits.HeaderSize];
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0, 4), magic);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4, 2), version);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(6, 2), 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(8, 4), payloadLength);
        return header;
    }
}
