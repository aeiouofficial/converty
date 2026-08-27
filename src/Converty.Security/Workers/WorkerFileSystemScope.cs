namespace Converty.Security.Workers;

public sealed class WorkerFileSystemScope
{
    public WorkerFileSystemScope(string writableDirectory)
    {
        if (string.IsNullOrWhiteSpace(writableDirectory) || !Path.IsPathFullyQualified(writableDirectory))
        {
            throw new ArgumentException("Worker writable directory must be fully qualified.", nameof(writableDirectory));
        }

        string fullPath = Path.GetFullPath(writableDirectory);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException("Worker writable directory does not exist.");
        }

        RejectReparsePointAncestry(fullPath);
        WritableDirectory = fullPath;
    }

    public string WritableDirectory { get; }

    private static void RejectReparsePointAncestry(string path)
    {
        DirectoryInfo? current = new(path);
        while (current is not null)
        {
            if ((File.GetAttributes(current.FullName) & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException("Worker writable directory ancestry must not contain a reparse point.");
            }

            current = current.Parent;
        }
    }
}
