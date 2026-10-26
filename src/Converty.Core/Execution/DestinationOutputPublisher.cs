namespace Converty.Core.Execution;

internal static class DestinationOutputPublisher
{
    private const int CopyBufferSize = 128 * 1024;

    public static bool TryPublish(string stagedOutputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stagedOutputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        string? outputDirectory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output path must have a destination directory.", nameof(outputPath));
        }

        string destinationTemporaryPath = Path.Combine(
            outputDirectory,
            $".converty-{Guid.NewGuid():N}.partial{Path.GetExtension(outputPath)}");

        try
        {
            using (var source = new FileStream(
                stagedOutputPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                CopyBufferSize,
                FileOptions.SequentialScan))
            using (var destination = new FileStream(
                destinationTemporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                CopyBufferSize,
                FileOptions.WriteThrough))
            {
                source.CopyTo(destination, CopyBufferSize);
                destination.Flush(flushToDisk: true);
            }

            File.Move(destinationTemporaryPath, outputPath, overwrite: false);
            return true;
        }
        catch (IOException) when (File.Exists(outputPath) || Directory.Exists(outputPath))
        {
            return false;
        }
        finally
        {
            TryDeleteTemporaryFile(destinationTemporaryPath);
        }
    }

    private static void TryDeleteTemporaryFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
