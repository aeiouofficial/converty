using System.Runtime.Versioning;
using Converty.Bridge.Ipc;

namespace Converty.Bridge.Startup;

[SupportedOSPlatform("windows")]
public static class PackagedBridgeRuntimeFactory
{
    public static BridgeSubmissionCoordinator CreateForCurrentUser(
        TimeSpan connectTimeout,
        TimeSpan startupTimeout,
        TimeSpan retryDelay)
    {
        TrustedHostPath trustedHost = TrustedHostPath.FromApplicationBaseDirectory();
        string packageFamilyName = WindowsCurrentPackageFamilyName.GetRequired();

        var verifier = new WindowsConnectedServerIdentityVerifier(
            trustedHost.ExecutablePath,
            packageFamilyName,
            new WindowsConnectedServerIdentityProbe());
        BridgeClient client = BridgeClient.ForCurrentUser(connectTimeout, verifier);
        var launcher = new InstalledHostProcessLauncher(trustedHost);

        return new BridgeSubmissionCoordinator(
            client,
            launcher,
            startupTimeout,
            retryDelay);
    }
}
