using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class WorkerFileSystemScopeTests
{
    [Fact]
    public void ConstructorRejectsReparsePointInWritableDirectoryAncestry()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        string root = Path.Combine(Path.GetTempPath(), $"ConvertyScope-{Guid.NewGuid():N}");
        string target = Path.Combine(root, "target");
        string link = Path.Combine(root, "link");
        string child = Path.Combine(link, "child");
        Directory.CreateDirectory(target);

        try
        {
            CreateDirectoryJunction(link, target);
            Directory.CreateDirectory(child);

            Assert.Throws<IOException>(() => new WorkerFileSystemScope(child));
        }
        finally
        {
            if (Directory.Exists(link))
            {
                Directory.Delete(link);
            }
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void CreateDirectoryJunction(string link, string target)
    {
        // A junction exercises the same reparse-point boundary without administrator privileges.
        var startInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe")
        {
            Arguments = $"/d /c mklink /J \"{link}\" \"{target}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };
        using var process = System.Diagnostics.Process.Start(startInfo);
        Assert.NotNull(process);
        Assert.True(process.WaitForExit(10_000), "Junction creation timed out.");
        Assert.True(process.ExitCode == 0, $"Junction creation failed: {process.StandardError.ReadToEnd()}");
        Assert.True((File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0);
    }
}
