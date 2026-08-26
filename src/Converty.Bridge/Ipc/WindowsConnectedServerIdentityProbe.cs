using System.IO.Pipes;
using System.Runtime.Versioning;

namespace Converty.Bridge.Ipc;

[SupportedOSPlatform("windows")]
public sealed class WindowsConnectedServerIdentityProbe : IConnectedServerIdentityProbe
{
    public ConnectedServerIdentitySnapshot Capture(NamedPipeClientStream pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        throw new BridgeServerIdentityException("Connected server native identity probe is not implemented.");
    }
}
