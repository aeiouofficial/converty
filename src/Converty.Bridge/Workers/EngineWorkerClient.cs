using Converty.Contracts.Identifiers;
using Converty.Core.Execution;
using Converty.Security.Workers;

namespace Converty.Bridge.Workers;

internal sealed class EngineWorkerClient(
    string workerExecutablePath,
    IWorkerProcessLauncher processLauncher) : IConversionWorkerClient
{
    internal const string WorkerExecutableFileName = "Converty.EngineWorker.exe";
    internal const int MaximumCapturedErrorCharacters = 64 * 1024;

    private readonly string _workerExecutablePath = ValidateWorkerExecutablePath(workerExecutablePath);
    private readonly IWorkerProcessLauncher _processLauncher =
        processLauncher ?? throw new ArgumentNullException(nameof(processLauncher));

    public static EngineWorkerClient CreateForApplicationBaseDirectory() =>
        new(
            ResolveWorkerExecutable(AppContext.BaseDirectory),
            new WindowsWorkerProcessLauncher());

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

        if (string.IsNullOrWhiteSpace(stagedInputPath) || !Path.IsPathFullyQualified(stagedInputPath))
        {
            throw new ArgumentException("Staged input path must be fully qualified.", nameof(stagedInputPath));
        }
        if (string.IsNullOrWhiteSpace(stagedOutputPath) || !Path.IsPathFullyQualified(stagedOutputPath))
        {
            throw new ArgumentException("Staged output path must be fully qualified.", nameof(stagedOutputPath));
        }

        string? stagingDirectory = Path.GetDirectoryName(Path.GetFullPath(stagedInputPath));
        string? outputDirectory = Path.GetDirectoryName(Path.GetFullPath(stagedOutputPath));
        if (stagingDirectory is null || outputDirectory is null ||
            !string.Equals(stagingDirectory, outputDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Staged worker input and output must share one private staging directory.");
        }

        string? workingDirectory = Path.GetDirectoryName(_workerExecutablePath);
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new InvalidOperationException("Worker executable path must have a parent directory.");
        }

        string[] arguments =
        [
            "--preset",
            presetId.Value,
            "--input",
            stagedInputPath,
            "--output",
            stagedOutputPath,
        ];
        var request = new WorkerProcessLaunchRequest(
            _workerExecutablePath,
            workingDirectory,
            arguments,
            WorkerIsolationLevel.Compatibility,
            WorkerResourceLimits.ConversionDefault,
            new WorkerFileSystemScope(stagingDirectory),
            timeout,
            MaximumCapturedErrorCharacters);
        WorkerProcessResult result = await _processLauncher.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        return new ConversionWorkerResult(result.ExitCode, result.StandardError);
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
        if (!string.Equals(
                Path.GetFileName(workerExecutablePath),
                WorkerExecutableFileName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Worker executable must use the fixed Converty worker filename.",
                nameof(workerExecutablePath));
        }
        return Path.GetFullPath(workerExecutablePath);
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
