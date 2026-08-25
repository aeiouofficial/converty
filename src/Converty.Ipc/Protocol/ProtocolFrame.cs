namespace Converty.Ipc.Protocol;

public readonly record struct ProtocolFrame(ushort Version, ReadOnlyMemory<byte> Payload);
