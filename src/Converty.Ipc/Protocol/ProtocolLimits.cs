namespace Converty.Ipc.Protocol;

public static class ProtocolLimits
{
    public const uint Magic = 0x59545643;
    public const ushort CurrentVersion = 1;
    public const int HeaderSize = 12;
    public const int MaxPayloadBytes = 1_048_576;
}
