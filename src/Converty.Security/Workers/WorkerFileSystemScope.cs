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

        RejectDirectoryReparsePointAncestry(fullPath);
        WritableDirectory = fullPath;
    }

    private WorkerFileSystemScope(string readOnlyFile, bool exactFile)
    {
        _ = exactFile;
        ReadOnlyFile = readOnlyFile;
    }

    public string? WritableDirectory { get; }

    public string? ReadOnlyFile { get; }

    public static WorkerFileSystemScope ForReadOnlyFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("Worker read-only file must be fully qualified.", nameof(path));
        }

        string fullPath = Path.GetFullPath(path);
        if (Directory.Exists(fullPath))
        {
            throw new ArgumentException("Worker read-only scope requires a file, not a directory.", nameof(path));
        }
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Worker read-only file does not exist.", fullPath);
        }

        RejectFileReparsePointAncestry(fullPath);
        return new WorkerFileSystemScope(fullPath, exactFile: true);
    }

    private static void RejectDirectoryReparsePointAncestry(string path)
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

    private static void RejectFileReparsePointAncestry(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Worker read-only file must not be a reparse point.");
        }

        DirectoryInfo? current = Directory.GetParent(path);
        while (current is not null)
        {
            if ((File.GetAttributes(current.FullName) & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException("Worker read-only file ancestry must not contain a reparse point.");
            }

            current = current.Parent;
        }
    }
}
