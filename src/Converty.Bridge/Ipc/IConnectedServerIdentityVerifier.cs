using System.IO.Pipes;

namespace Converty.Bridge.Ipc;

public interface IConnectedServerIdentityVerifier
{
    void VerifyConnectedServer(NamedPipeClientStream pipe);
}
