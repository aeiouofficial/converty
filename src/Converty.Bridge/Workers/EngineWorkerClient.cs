using System.Diagnostics;
using System.Text;
using Converty.Contracts.Identifiers;
using Converty.Core.Execution;

namespace Converty.Bridge.Workers;

internal sealed class EngineWorkerClient(string workerExecutablePath) : IConversionWorkerClient
{
    internal const string WorkerExecutableFileName = "Converty.EngineWorker.exe";
    internal const int MaximumCapturedErrorCharacters = 64 * 1024;

    private readonly string _workerExecutablePath = ValidateWorkerExecutablePath(workerExecutablePath);

    public static EngineWorkerClient CreateForApplicationBaseDirectory() =>
        new(ResolveWorkerExecutable(AppContext.BaseDirectory));

    public async Task<ConversionWorkerResult> ExecuteAsync(
        PresetId presetId,
        string stagedInputPath,
        string stagedOutputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presetId);
        ValidateTimeout(timeout);
        cancellationToken.ThrowIfCancellationRequested();

        ProcessStartInfo startInfo = CreateStartInfo(
            _workerExecutablePath,
            presetId,
            stagedInputPath,
            stagedOutputPath);
        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException("Converty conversion worker could not be started.");
        }

        Task<string> standardErrorTask = DrainAsync(process.StandardError, MaximumCapturedErrorCharacters);
        Task<string> standardOutputTask = DrainAsync(process.StandardOutput, maximumCharacters: 0);
        using CancellationTokenSource timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
            throw new TimeoutException("Conversion worker exceeded the configured execution timeout.");
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(process);
            await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
            throw;
        }

        string standardError = await standardErrorTask.ConfigureAwait(false);
        _ = await standardOutputTask.ConfigureAwait(false);
        return new ConversionWorkerResult(process.ExitCode, standardError);
    }

    internal static ProcessStartInfo CreateStartInfo(
        string workerExecutablePath,
        PresetId presetId,
        string stagedInputPath,
        string stagedOutputPath)
    {
        string validatedWorkerPath = ValidateWorkerExecutablePath(workerExecutablePath);
        if (string.IsNullOrWhiteSpace(stagedInputPath) || !Path.IsPathFullyQualified(stagedInputPath))
        {
            throw new ArgumentException("Staged input path must be fully qualified.", nameof(stagedInputPath));
        }
        if (string.IsNullOrWhiteSpace(stagedOutputPath) || !Path.IsPathFullyQualified(stagedOutputPath))
        {
            throw new ArgumentException("Staged output path must be fully qualified.", nameof(stagedOutputPath));
        }

        string? workingDirectory = Path.GetDirectoryName(validatedWorkerPath);
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new ArgumentException("Worker executable path must have a parent directory.", nameof(workerExecutablePath));
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = validatedWorkerPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = workingDirectory,
        };
        startInfo.ArgumentList.Add("--preset");
        startInfo.ArgumentList.Add(presetId.Value);
        startInfo.ArgumentList.Add("--input");
        startInfo.ArgumentList.Add(stagedInputPath);
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(stagedOutputPath);
        return startInfo;
    }

    private static string ResolveWorkerExecutable(string applicationDirectory)
    {
        if (string.IsNullOrWhiteSpace(applicationDirectory))
        {
            throw new ArgumentException("Application directory is required.", nameof(applicationDirectory));
        }

        string root = Path.GetFullPath(applicationDirectory);
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException("Converty application directory does not exist.");
        }
        RejectReparsePoint(root, "Converty application directory");

        string workerPath = Path.GetFullPath(Path.Combine(root, WorkerExecutableFileName));
        if (!File.Exists(workerPath))
        {
            throw new FileNotFoundException("Bundled Converty conversion worker is missing.", workerPath);
        }
        RejectReparsePoint(workerPath, "Bundled Converty conversion worker");
        return workerPath;
    }

    private static string ValidateWorkerExecutablePath(string workerExecutablePath)
    {
        if (string.IsNullOrWhiteSpace(workerExecutablePath) || !Path.IsPathFullyQualified(workerExecutablePath))
        {
            throw new ArgumentException("Worker executable path must be fully qualified.", nameof(workerExecutablePath));
        }
        if (!string.Equals(Path.GetFileName(workerExecutablePath), WorkerExecutableFileName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Worker executable must use the fixed Converty worker filename.", nameof(workerExecutablePath));
        }
        return Path.GetFullPath(workerExecutablePath);
    }

    private static async Task<string> DrainAsync(StreamReader reader, int maximumCharacters)
    {
        var captured = new StringBuilder(Math.Min(maximumCharacters, 4096));
        char[] buffer = new char[4096];
        while (true)
        {
            int read = await reader.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            int remaining = maximumCharacters - captured.Length;
            if (remaining > 0)
            {
                captured.Append(buffer, 0, Math.Min(read, remaining));
            }
        }
        return captured.ToString();
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task IgnoreDrainFailuresAsync(params Task<string>[] drains)
    {
        try
        {
            await Task.WhenAll(drains).ConfigureAwait(false);
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private static void ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout > ConversionBatchRunner.MaximumExecutionTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }
    }

    private static void RejectReparsePoint(string path, string description)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException($"{description} must not be a reparse point.");
        }
    }
}
