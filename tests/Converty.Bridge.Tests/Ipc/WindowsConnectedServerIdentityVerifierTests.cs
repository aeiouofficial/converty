using System.IO.Pipes;
using Converty.Bridge.Ipc;

namespace Converty.Bridge.Tests.Ipc;

public sealed class WindowsConnectedServerIdentityVerifierTests
{
    private const string ExpectedFamily = "Converty.Test_abcd1234";
    private const string ExpectedPath = @"C:\Program Files\WindowsApps\Converty.Test_1.0.0.0_x64__abcd1234\Converty.Host.exe";

    [Fact]
    public void AcceptsStableExpectedHostPathAndPackageFamily()
    {
        var probe = new FakeProbe(new ConnectedServerIdentitySnapshot(42, ExpectedPath, ExpectedFamily, 42));
        var verifier = new WindowsConnectedServerIdentityVerifier(ExpectedPath, ExpectedFamily, probe);

        verifier.VerifySnapshot(probe.Snapshot);
    }

    [Fact]
    public void RejectsWrongHostImagePath()
    {
        var probe = new FakeProbe(new ConnectedServerIdentitySnapshot(42, @"C:\Temp\Converty.Host.exe", ExpectedFamily, 42));
        var verifier = new WindowsConnectedServerIdentityVerifier(ExpectedPath, ExpectedFamily, probe);

        BridgeServerIdentityException error = Assert.Throws<BridgeServerIdentityException>(
            () => verifier.VerifySnapshot(probe.Snapshot));

        Assert.Contains("path", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RejectsWrongOrMissingPackageFamily()
    {
        var wrong = new WindowsConnectedServerIdentityVerifier(
            ExpectedPath,
            ExpectedFamily,
            new FakeProbe(new ConnectedServerIdentitySnapshot(42, ExpectedPath, "Other.Publisher_xyz", 42)));
        var missing = new WindowsConnectedServerIdentityVerifier(
            ExpectedPath,
            ExpectedFamily,
            new FakeProbe(new ConnectedServerIdentitySnapshot(42, ExpectedPath, null, 42)));

        Assert.Throws<BridgeServerIdentityException>(() => wrong.VerifySnapshot(((FakeProbe)wrong.ProbeForTests).Snapshot));
        Assert.Throws<BridgeServerIdentityException>(() => missing.VerifySnapshot(((FakeProbe)missing.ProbeForTests).Snapshot));
    }

    [Fact]
    public void RejectsServerPidRace()
    {
        var snapshot = new ConnectedServerIdentitySnapshot(42, ExpectedPath, ExpectedFamily, 43);
        var probe = new FakeProbe(snapshot);
        var verifier = new WindowsConnectedServerIdentityVerifier(ExpectedPath, ExpectedFamily, probe);

        BridgeServerIdentityException error = Assert.Throws<BridgeServerIdentityException>(
            () => verifier.VerifySnapshot(snapshot));

        Assert.Contains("process", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VerifyConnectedServerUsesFreshProbeForEverySession()
    {
        var probe = new FakeProbe(new ConnectedServerIdentitySnapshot(42, ExpectedPath, ExpectedFamily, 42));
        var verifier = new WindowsConnectedServerIdentityVerifier(ExpectedPath, ExpectedFamily, probe);
        using var pipe = new NamedPipeClientStream("unused");

        verifier.VerifyConnectedServer(pipe);
        verifier.VerifyConnectedServer(pipe);

        Assert.Equal(2, probe.CaptureCount);
    }

    private sealed class FakeProbe(ConnectedServerIdentitySnapshot snapshot) : IConnectedServerIdentityProbe
    {
        public ConnectedServerIdentitySnapshot Snapshot { get; } = snapshot;
        public int CaptureCount { get; private set; }

        public ConnectedServerIdentitySnapshot Capture(NamedPipeClientStream pipe)
        {
            CaptureCount++;
            return Snapshot;
        }
    }
}
