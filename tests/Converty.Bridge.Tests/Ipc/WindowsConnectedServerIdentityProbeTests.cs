using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Bridge.Ipc;

namespace Converty.Bridge.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class WindowsConnectedServerIdentityProbeTests
{
    [Fact]
    public async Task CaptureRejectsUnpackagedServerAtPackageIdentityBoundary()
    {
        string pipeName = $"Converty-Probe-{Guid.NewGuid():N}";
        using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous,
            TokenImpersonationLevel.Impersonation);

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Task accept = server.WaitForConnectionAsync(cancellationToken);
        await client.ConnectAsync(2_000, cancellationToken);
        await accept;

        var probe = new WindowsConnectedServerIdentityProbe();
        BridgeServerIdentityException error = Assert.Throws<BridgeServerIdentityException>(
            () => probe.Capture(client));

        Assert.Contains("package", error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
