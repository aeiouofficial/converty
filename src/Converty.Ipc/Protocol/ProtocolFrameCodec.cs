using System.Buffers.Binary;

namespace Converty.Ipc.Protocol;

public static class ProtocolFrameCodec
{
    public static async ValueTask WriteAsync(
        Stream stream,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        cancellationToken.ThrowIfCancellationRequested();

        if (payload.Length > ProtocolLimits.MaxPayloadBytes)
        {
            throw new ProtocolException(
                ProtocolErrorCode.FrameTooLarge,
                $"IPC payload length {payload.Length} exceeds limit {ProtocolLimits.MaxPayloadBytes}.");
        }

        byte[] header = new byte[ProtocolLimits.HeaderSize];
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0, 4), ProtocolLimits.Magic);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4, 2), ProtocolLimits.CurrentVersion);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(6, 2), 0);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(8, 4), payload.Length);

        await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        if (!payload.IsEmpty)
        {
            await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        }
    }

    public static async ValueTask<ProtocolFrame> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        cancellationToken.ThrowIfCancellationRequested();

        byte[] header = new byte[ProtocolLimits.HeaderSize];
        await ReadExactAsync(stream, header, cancellationToken).ConfigureAwait(false);

        uint magic = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0, 4));
        if (magic != ProtocolLimits.Magic)
        {
            throw new ProtocolException(ProtocolErrorCode.BadMagic, "IPC frame magic is invalid.");
        }

        ushort version = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(4, 2));
        if (version != ProtocolLimits.CurrentVersion)
        {
            throw new ProtocolException(
                ProtocolErrorCode.UnsupportedVersion,
                $"IPC protocol version {version} is not supported.");
        }

        int payloadLength = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(8, 4));
        if (payloadLength < 0)
        {
            throw new ProtocolException(ProtocolErrorCode.InvalidLength, "IPC payload length cannot be negative.");
        }

        if (payloadLength > ProtocolLimits.MaxPayloadBytes)
        {
            throw new ProtocolException(
                ProtocolErrorCode.FrameTooLarge,
                $"IPC payload length {payloadLength} exceeds limit {ProtocolLimits.MaxPayloadBytes}.");
        }

        byte[] payload = new byte[payloadLength];
        if (payloadLength != 0)
        {
            await ReadExactAsync(stream, payload, cancellationToken).ConfigureAwait(false);
        }

        return new ProtocolFrame(version, payload);
    }

    private static async ValueTask ReadExactAsync(
        Stream stream,
        Memory<byte> destination,
        CancellationToken cancellationToken)
    {
        int totalRead = 0;
        while (totalRead < destination.Length)
        {
            int read = await stream.ReadAsync(destination[totalRead..], cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new ProtocolException(ProtocolErrorCode.TruncatedFrame, "IPC frame ended before the declared length.");
            }

            totalRead = checked(totalRead + read);
        }
    }
}
