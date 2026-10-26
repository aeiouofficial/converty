using Converty.Contracts.Conversion;
using Converty.Core.Execution;
using Converty.Security.Workers;
using Converty.Serialization;

namespace Converty.Bridge.Workers;

internal sealed class ProbeWorkerClient(
    string workerExecutablePath,
    IWorkerProcessLauncher processLauncher) : IMediaProbeClient
{
    internal const string WorkerExecutableFileName = "Converty.ProbeWorker.exe";
    internal const int MaximumCapturedErrorCharacters = 16 * 1024;
    internal const int MaximumCapturedStandardOutputBytes = 256 * 1024;

    private readonly string _workerExecutablePath = ValidateWorkerExecutablePath(workerExecutablePath);
    private readonly IWorkerProcessLauncher _processLauncher =
        processLauncher ?? throw new ArgumentNullException(nameof(processLauncher));

    public static ProbeWorkerClient CreateForApplicationBaseDirectory() =>
        new(
            ResolveWorkerExecutable(AppContext.BaseDirectory),
            new WindowsWorkerProcessLauncher());

    public async Task<MediaProbeResultV1> ProbeAsync(
        string stagedInputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ValidateTimeout(timeout);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(stagedInputPath) || !Path.IsPathFullyQualified(stagedInputPath))
        {
            throw new ArgumentException("Staged probe input path must be fully qualified.", nameof(stagedInputPath));
        }

        string fullInputPath = Path.GetFullPath(stagedInputPath);
        WorkerFileSystemScope fileSystemScope = WorkerFileSystemScope.ForReadOnlyFile(fullInputPath);
        string workingDirectory = Path.GetDirectoryName(_workerExecutablePath) ??
            throw new InvalidOperationException("ProbeWorker executable path must have a parent directory.");

        var request = new WorkerProcessLaunchRequest(
            _workerExecutablePath,
            workingDirectory,
            ["--input", fullInputPath],
            WorkerIsolationLevel.Strict,
            WorkerResourceLimits.ConversionDefault,
            fileSystemScope,
            timeout,
            MaximumCapturedErrorCharacters,
            MaximumCapturedStandardOutputBytes);

        WorkerProcessResult result = await _processLauncher.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            return MediaProbeResultV1.Failure(MediaProbeFailureReason.ProbeFailed);
        }

        return ContractJson.DeserializeMediaProbeResult(result.StandardOutput);
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
            throw new FileNotFoundException("Bundled Converty probe worker is missing.", workerPath);
        }
        RejectReparsePoint(workerPath, "Bundled Converty probe worker");
        return workerPath;
    }

    private static string ValidateWorkerExecutablePath(string workerExecutablePath)
    {
        if (string.IsNullOrWhiteSpace(workerExecutablePath) || !Path.IsPathFullyQualified(workerExecutablePath))
        {
            throw new ArgumentException("ProbeWorker executable path must be fully qualified.", nameof(workerExecutablePath));
        }
        if (!string.Equals(
                Path.GetFileName(workerExecutablePath),
                WorkerExecutableFileName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "ProbeWorker executable must use the fixed Converty probe-worker filename.",
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
