using System.Diagnostics;
using System.Runtime.Versioning;
using Converty.Bridge.Startup;

namespace Converty.Bridge.Tests.Startup;

[SupportedOSPlatform("windows")]
public sealed class TrustedHostStartupTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "converty-bridge-startup-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void TrustedPathUsesOnlyFixedHostExecutableInsideExistingAbsoluteInstallDirectory()
    {
        Directory.CreateDirectory(_root);
        string expected = Path.Combine(_root, "Converty.Host.exe");
        File.WriteAllBytes(expected, [0x4d, 0x5a]);

        var trusted = new TrustedHostPath(_root);

        Assert.Equal(Path.GetFullPath(_root), trusted.InstallDirectory);
        Assert.Equal(expected, trusted.ExecutablePath);
    }

    [Fact]
    public void TrustedPathRejectsRelativeMissingAndWrongFilenameOnlyLayouts()
    {
        Assert.Throws<ArgumentException>(() => new TrustedHostPath("relative-install"));

        string missing = Path.Combine(_root, "missing");
        Assert.Throws<DirectoryNotFoundException>(() => new TrustedHostPath(missing));

        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "not-the-host.exe"), "x");
        Assert.Throws<FileNotFoundException>(() => new TrustedHostPath(_root));
    }

    [Fact]
    public void LauncherStartInfoHasNoShellNoArgumentsAndNoConsole()
    {
        Directory.CreateDirectory(_root);
        string executable = Path.Combine(_root, "Converty.Host.exe");
        File.WriteAllBytes(executable, [0x4d, 0x5a]);
        var trusted = new TrustedHostPath(_root);

        ProcessStartInfo startInfo = InstalledHostProcessLauncher.CreateStartInfo(trusted);

        Assert.Equal(executable, startInfo.FileName);
        Assert.Equal(string.Empty, startInfo.Arguments);
        Assert.False(startInfo.UseShellExecute);
        Assert.True(startInfo.CreateNoWindow);
        Assert.Equal(ProcessWindowStyle.Hidden, startInfo.WindowStyle);
        Assert.Equal(Path.GetFullPath(_root), startInfo.WorkingDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
