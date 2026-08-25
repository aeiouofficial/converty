namespace Converty.Ipc.Protocol;

public enum ProtocolErrorCode
{
    BadMagic = 1,
    UnsupportedVersion = 2,
    InvalidLength = 3,
    FrameTooLarge = 4,
    TruncatedFrame = 5,
}
