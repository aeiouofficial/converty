namespace Converty.Core.Execution;

internal sealed record ConversionStagingPaths(
    string DirectoryPath,
    string InputPath,
    string OutputPath);

internal static class ConversionStagingDirectory
{
    private const string ProductDirectoryName = "Converty";
    private const string StagingDirectoryName = "WorkerStaging";

    public static ConversionStagingPaths Create(string sourcePath, string outputExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputExtension);

        string stagingRoot = ResolveStagingRoot();
        Directory.CreateDirectory(stagingRoot);

        string jobDirectory = Path.Combine(stagingRoot, "job-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(jobDirectory);

        try
        {
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
        if (string.IsNullOrWhiteSpace(jobDirectory))
        {
            return;
        }

        try
        {
            if (Directory.Exists(jobDirectory))
            {
                Directory.Delete(jobDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
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
