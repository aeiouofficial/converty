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
            Directory.CreateSymbolicLink(link, target);
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
}
