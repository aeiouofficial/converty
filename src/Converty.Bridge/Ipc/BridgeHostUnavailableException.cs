namespace Converty.Bridge.Ipc;

public sealed class BridgeHostUnavailableException : IOException
{
    public BridgeHostUnavailableException()
        : base("Converty Host is unavailable.")
    {
    }

    public BridgeHostUnavailableException(string message)
        : base(message)
    {
    }

    public BridgeHostUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
