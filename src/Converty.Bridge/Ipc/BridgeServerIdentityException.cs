namespace Converty.Bridge.Ipc;

public sealed class BridgeServerIdentityException : Exception
{
    public BridgeServerIdentityException(string message)
        : base(message)
    {
    }

    public BridgeServerIdentityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
