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
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Worker writable directory must not be a reparse point.");
        }

        WritableDirectory = fullPath;
    }

    public string WritableDirectory { get; }
}
