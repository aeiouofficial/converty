using System.IO.Pipes;

namespace Converty.Bridge.Ipc;

public interface IConnectedServerIdentityProbe
{
    ConnectedServerIdentitySnapshot Capture(NamedPipeClientStream pipe);
}
