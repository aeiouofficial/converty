namespace Converty.Bridge.Startup;

public sealed class TrustedHostPath
{
    public const string HostExecutableFileName = "Converty.Host.exe";

    public TrustedHostPath(string installDirectory)
    {
        if (string.IsNullOrWhiteSpace(installDirectory))
        {
            throw new ArgumentException("Install directory is required.", nameof(installDirectory));
        }

        if (!Path.IsPathFullyQualified(installDirectory))
        {
            throw new ArgumentException("Install directory must be an absolute path.", nameof(installDirectory));
        }

        string fullDirectory = Path.GetFullPath(installDirectory);
        if (!Directory.Exists(fullDirectory))
        {
            throw new DirectoryNotFoundException($"Trusted Converty install directory does not exist: {fullDirectory}");
        }

        var directoryInfo = new DirectoryInfo(fullDirectory);
        if ((directoryInfo.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Trusted Converty install directory must not be a reparse point.");
        }

        string executablePath = Path.Combine(fullDirectory, HostExecutableFileName);
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException("Trusted Converty Host executable was not found.", executablePath);
        }

        var executableInfo = new FileInfo(executablePath);
        if ((executableInfo.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Trusted Converty Host executable must not be a reparse point.");
        }

        InstallDirectory = fullDirectory;
        ExecutablePath = executablePath;
    }

    public string InstallDirectory { get; }

    public string ExecutablePath { get; }

    public static TrustedHostPath FromApplicationBaseDirectory() => new(AppContext.BaseDirectory);
}
