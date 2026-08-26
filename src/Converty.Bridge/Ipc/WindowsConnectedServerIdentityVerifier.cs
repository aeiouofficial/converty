using System.IO.Pipes;

namespace Converty.Bridge.Ipc;

public sealed class WindowsConnectedServerIdentityVerifier : IConnectedServerIdentityVerifier
{
    private readonly string _expectedHostPath;
    private readonly string _expectedPackageFamilyName;
    private readonly IConnectedServerIdentityProbe _probe;

    public WindowsConnectedServerIdentityVerifier(
        string expectedHostPath,
        string expectedPackageFamilyName,
        IConnectedServerIdentityProbe probe)
    {
        if (string.IsNullOrWhiteSpace(expectedHostPath))
        {
            throw new ArgumentException("Expected Host path is required.", nameof(expectedHostPath));
        }

        if (string.IsNullOrWhiteSpace(expectedPackageFamilyName))
        {
            throw new ArgumentException("Expected package family is required.", nameof(expectedPackageFamilyName));
        }

        _expectedHostPath = expectedHostPath;
        _expectedPackageFamilyName = expectedPackageFamilyName;
        _probe = probe ?? throw new ArgumentNullException(nameof(probe));
    }

    public void VerifyConnectedServer(NamedPipeClientStream pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        VerifySnapshot(_probe.Capture(pipe));
    }

    public void VerifySnapshot(ConnectedServerIdentitySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _ = _expectedHostPath;
        _ = _expectedPackageFamilyName;
        throw new BridgeServerIdentityException("Connected server identity verification is not implemented.");
    }
}
