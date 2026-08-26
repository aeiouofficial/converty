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

        try
        {
            VerifySnapshot(_probe.Capture(pipe));
        }
        catch (BridgeServerIdentityException)
        {
            throw;
        }
        catch (Exception error) when (error is IOException
                                      or UnauthorizedAccessException
                                      or InvalidOperationException
                                      or System.Security.SecurityException
                                      or System.ComponentModel.Win32Exception)
        {
            throw new BridgeServerIdentityException(
                "Unable to establish the connected Converty Host identity.",
                error);
        }
    }

    public void VerifySnapshot(ConnectedServerIdentitySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.ServerProcessId == 0
            || snapshot.ConfirmedServerProcessId == 0
            || snapshot.ServerProcessId != snapshot.ConfirmedServerProcessId)
        {
            throw new BridgeServerIdentityException(
                "Connected server process identity changed during verification.");
        }

        if (!string.Equals(snapshot.ImagePath, _expectedHostPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new BridgeServerIdentityException(
                "Connected server image path does not match the trusted Converty Host path.");
        }

        if (string.IsNullOrWhiteSpace(snapshot.PackageFamilyName)
            || !string.Equals(
                snapshot.PackageFamilyName,
                _expectedPackageFamilyName,
                StringComparison.Ordinal))
        {
            throw new BridgeServerIdentityException(
                "Connected server package family does not match the expected Converty package family.");
        }
    }
}
