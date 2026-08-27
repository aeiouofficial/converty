namespace Converty.Provider.FFmpeg;

public static class TrustedFfmpegPath
{
    public const string ExecutableFileName = "ffmpeg.exe";

    public static string Resolve(string applicationDirectory)
    {
        if (string.IsNullOrWhiteSpace(applicationDirectory))
        {
            throw new ArgumentException("Application directory is required.", nameof(applicationDirectory));
        }

        string root = Path.GetFullPath(applicationDirectory);
        if (!Path.IsPathFullyQualified(root) || !Directory.Exists(root))
        {
            throw new DirectoryNotFoundException("Converty application directory does not exist.");
        }

        RejectReparsePoint(root, "Converty application directory");

        string toolsDirectory = Path.Combine(root, "tools");
        if (Directory.Exists(toolsDirectory))
        {
            RejectReparsePoint(toolsDirectory, "Bundled Converty tools directory");
        }

        string engineDirectory = Path.Combine(toolsDirectory, "ffmpeg");
        if (!Directory.Exists(engineDirectory))
        {
            throw new FileNotFoundException(
                "Bundled Converty FFmpeg directory is missing.",
                Path.Combine(engineDirectory, ExecutableFileName));
        }

        RejectReparsePoint(engineDirectory, "Bundled Converty FFmpeg directory");

        string executablePath = Path.GetFullPath(Path.Combine(engineDirectory, ExecutableFileName));
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException("Bundled Converty ffmpeg.exe is missing.", executablePath);
        }

        RejectReparsePoint(executablePath, "Bundled Converty ffmpeg.exe");
        return executablePath;
    }

    public static string ResolveFromApplicationBaseDirectory() => Resolve(AppContext.BaseDirectory);

    private static void RejectReparsePoint(string path, string description)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException($"{description} must not be a reparse point.");
        }
    }
}
