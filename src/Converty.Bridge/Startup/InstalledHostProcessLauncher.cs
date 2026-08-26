using System.Diagnostics;
using System.Runtime.Versioning;

namespace Converty.Bridge.Startup;

[SupportedOSPlatform("windows")]
public sealed class InstalledHostProcessLauncher : IHostProcessLauncher
{
    private readonly TrustedHostPath _trustedHostPath;

    public InstalledHostProcessLauncher(TrustedHostPath trustedHostPath)
    {
        _trustedHostPath = trustedHostPath ?? throw new ArgumentNullException(nameof(trustedHostPath));
    }

    public void StartHost()
    {
        ProcessStartInfo startInfo = CreateStartInfo(_trustedHostPath);
        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Trusted Converty Host process could not be started.");
    }

    public static ProcessStartInfo CreateStartInfo(TrustedHostPath trustedHostPath)
    {
        ArgumentNullException.ThrowIfNull(trustedHostPath);

        return new ProcessStartInfo
        {
            FileName = trustedHostPath.ExecutablePath,
            Arguments = string.Empty,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = trustedHostPath.InstallDirectory,
        };
    }
}
