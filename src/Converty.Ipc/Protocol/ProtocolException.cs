namespace Converty.Ipc.Protocol;

public sealed class ProtocolException : IOException
{
    public ProtocolException(ProtocolErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ProtocolErrorCode ErrorCode { get; }
}
