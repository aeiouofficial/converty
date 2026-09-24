using System.Globalization;

namespace Converty.Core.Execution;

internal sealed record ConversionStagingPaths(
    string DirectoryPath,
    string InputPath,
    string OutputPath);

internal static class ConversionStagingDirectory
{
    private const string ProductDirectoryName = "Converty";
    private const string StagingDirectoryName = "WorkerStaging";
    private const string OwnershipMarkerName = ".converty-owned";
    private const string JobDirectoryPrefix = "job-";

    internal static readonly TimeSpan StaleJobAge = TimeSpan.FromHours(24);

    public static ConversionStagingPaths Create(string sourcePath, string outputExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputExtension);

        string stagingRoot = ResolveStagingRoot();
        Directory.CreateDirectory(stagingRoot);
        CleanupStaleOwnedJobs(stagingRoot);

        string jobDirectory = Path.Combine(stagingRoot, JobDirectoryPrefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(jobDirectory);

        try
        {
            WriteOwnershipMarker(jobDirectory);

            string inputExtension = Path.GetExtension(sourcePath);
            string stagedInputPath = Path.Combine(jobDirectory, "input" + inputExtension);
            string stagedOutputPath = Path.Combine(jobDirectory, "output.partial" + outputExtension);
            File.Copy(sourcePath, stagedInputPath, overwrite: false);
            return new ConversionStagingPaths(jobDirectory, stagedInputPath, stagedOutputPath);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            DeleteOwned(jobDirectory);
            throw;
        }
    }

    public static void DeleteOwned(string jobDirectory)
    {
        if (!TryGetOwnedJobDirectory(jobDirectory, out string ownedDirectory))
        {
            return;
        }

        TryDeleteTreeWithoutFollowingReparsePoints(ownedDirectory);
    }

    internal static void CleanupStaleOwnedJobs()
    {
        CleanupStaleOwnedJobs(ResolveStagingRoot());
    }

    private static void CleanupStaleOwnedJobs(string stagingRoot)
    {
        if (!Directory.Exists(stagingRoot))
        {
            return;
        }

        string[] candidates;
        try
        {
            candidates = Directory.GetDirectories(stagingRoot, JobDirectoryPrefix + "*", SearchOption.TopDirectoryOnly);
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        DateTime cutoffUtc = DateTime.UtcNow - StaleJobAge;
        foreach (string candidate in candidates)
        {
            if (!TryGetOwnedJobDirectory(candidate, out string ownedDirectory))
            {
                continue;
            }

            string markerPath = Path.Combine(ownedDirectory, OwnershipMarkerName);
            try
            {
                if (File.GetLastWriteTimeUtc(markerPath) > cutoffUtc)
                {
                    continue;
                }
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            TryDeleteTreeWithoutFollowingReparsePoints(ownedDirectory);
        }
    }

    private static void WriteOwnershipMarker(string jobDirectory)
    {
        string markerPath = Path.Combine(jobDirectory, OwnershipMarkerName);
        File.WriteAllText(
            markerPath,
            DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
    }

    private static bool TryGetOwnedJobDirectory(string jobDirectory, out string ownedDirectory)
    {
        ownedDirectory = string.Empty;
        if (string.IsNullOrWhiteSpace(jobDirectory))
        {
            return false;
        }

        string stagingRoot;
        string candidate;
        try
        {
            stagingRoot = Path.GetFullPath(ResolveStagingRoot());
            candidate = Path.GetFullPath(jobDirectory);
        }
        catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }

        DirectoryInfo? parent = Directory.GetParent(candidate);
        if (parent is null
            || !string.Equals(parent.FullName, stagingRoot, StringComparison.OrdinalIgnoreCase)
            || !Path.GetFileName(candidate).StartsWith(JobDirectoryPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            if (!Directory.Exists(candidate)
                || (File.GetAttributes(candidate) & FileAttributes.ReparsePoint) != 0)
            {
                return false;
            }

            string markerPath = Path.Combine(candidate, OwnershipMarkerName);
            if (!File.Exists(markerPath)
                || (File.GetAttributes(markerPath) & FileAttributes.ReparsePoint) != 0)
            {
                return false;
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        ownedDirectory = candidate;
        return true;
    }

    private static void TryDeleteTreeWithoutFollowingReparsePoints(string directoryPath)
    {
        try
        {
            DeleteTreeWithoutFollowingReparsePoints(new DirectoryInfo(directoryPath));
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void DeleteTreeWithoutFollowingReparsePoints(DirectoryInfo directory)
    {
        if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            Directory.Delete(directory.FullName, recursive: false);
            return;
        }

        foreach (FileSystemInfo entry in directory.EnumerateFileSystemInfos())
        {
            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                if ((entry.Attributes & FileAttributes.Directory) != 0)
                {
                    Directory.Delete(entry.FullName, recursive: false);
                }
                else
                {
                    File.Delete(entry.FullName);
                }

                continue;
            }

            if (entry is DirectoryInfo childDirectory)
            {
                DeleteTreeWithoutFollowingReparsePoints(childDirectory);
            }
            else
            {
                File.Delete(entry.FullName);
            }
        }

        Directory.Delete(directory.FullName, recursive: false);
    }

    private static string ResolveStagingRoot()
    {
        string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string authorityRoot = string.IsNullOrWhiteSpace(localApplicationData)
            ? Path.GetTempPath()
            : localApplicationData;
        return Path.Combine(authorityRoot, ProductDirectoryName, StagingDirectoryName);
    }
}
