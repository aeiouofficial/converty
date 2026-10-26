using System.Security.Cryptography;

namespace Converty.Core.Execution;

public static class ManagedByteCopy
{
    private const int BufferSize = 128 * 1024;

    public static async Task<string> CopyAndVerifyAsync(
        string inputPath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        string input = ValidateInputPath(inputPath);
        string output = ValidateOutputPath(outputPath, input);
        cancellationToken.ThrowIfCancellationRequested();

        bool outputCreated = false;
        bool verified = false;
        try
        {
            await using FileStream inputStream = new(
                input,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using (FileStream outputStream = new(
                output,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                outputCreated = true;
                await inputStream.CopyToAsync(outputStream, BufferSize, cancellationToken).ConfigureAwait(false);
                await outputStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            inputStream.Position = 0;
            byte[] inputHash = await SHA256.HashDataAsync(inputStream, cancellationToken).ConfigureAwait(false);
            await using FileStream outputHashStream = new(
                output,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            byte[] outputHash = await SHA256.HashDataAsync(outputHashStream, cancellationToken).ConfigureAwait(false);
            if (!CryptographicOperations.FixedTimeEquals(inputHash, outputHash))
            {
                throw new IOException("Managed byte copy failed SHA-256 equality verification.");
            }

            verified = true;
            return Convert.ToHexString(inputHash).ToLowerInvariant();
        }
        finally
        {
            if (outputCreated && !verified)
            {
                DeletePartialOutput(output);
            }
        }
    }

    private static string ValidateInputPath(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || !Path.IsPathFullyQualified(inputPath))
        {
            throw new ArgumentException("Copy input path must be fully qualified.", nameof(inputPath));
        }

        string fullPath = Path.GetFullPath(inputPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Copy input file is missing.", fullPath);
        }
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Copy input must not be a reparse point.");
        }

        return fullPath;
    }

    private static string ValidateOutputPath(string outputPath, string inputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath) || !Path.IsPathFullyQualified(outputPath))
        {
            throw new ArgumentException("Copy output path must be fully qualified.", nameof(outputPath));
        }

        string fullPath = Path.GetFullPath(outputPath);
        if (string.Equals(fullPath, inputPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Copy input and output paths must differ.", nameof(outputPath));
        }

        string? inputDirectory = Path.GetDirectoryName(inputPath);
        string? outputDirectory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(inputDirectory) ||
            string.IsNullOrWhiteSpace(outputDirectory) ||
            !string.Equals(inputDirectory, outputDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Managed copy input and output must share one private staging directory.");
        }

        return fullPath;
    }

    private static void DeletePartialOutput(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
