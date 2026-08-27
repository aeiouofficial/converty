using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Converty.Security.Workers;

public sealed class WindowsWorkerProcessLauncher : IWorkerProcessLauncher
{
    public static readonly TimeSpan MaximumExecutionTimeout = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan OutputPollInterval = TimeSpan.FromMilliseconds(25);
    public const int MaximumArgumentCount = 16;
    public const int MaximumArgumentCharacters = 32_767;
    public const int MaximumCapturedErrorCharacters = 64 * 1024;

    public async Task<WorkerProcessResult> ExecuteAsync(
        WorkerProcessLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyDictionary<string, long> stagingBaseline =
            CaptureFileLengths(request.FileSystemScope.WritableDirectory);
        WindowsAppContainerProfile? appContainer = null;
        WindowsAclGrant? applicationGrant = null;
        WindowsAclGrant? stagingGrant = null;
        WindowsJobObject? job = null;
        WindowsSuspendedProcess? worker = null;
        Process? process = null;
        Task<string>? standardErrorTask = null;
        Task<string>? standardOutputTask = null;

        try
        {
            if (request.IsolationLevel == WorkerIsolationLevel.Strict)
            {
                appContainer = WindowsAppContainerProfile.Create();
                string applicationDirectory = Path.GetDirectoryName(request.ExecutablePath) ??
                    throw new InvalidOperationException("Strict worker executable requires an application directory.");
                applicationGrant = WindowsAclGrant.GrantApplicationReadExecute(applicationDirectory, appContainer.Sid);
                stagingGrant = WindowsAclGrant.GrantStagingReadWrite(
                    request.FileSystemScope.WritableDirectory,
                    appContainer.Sid);
            }

            job = WindowsJobObject.Create(request.ResourceLimits);
            worker = WindowsSuspendedProcess.Create(request, appContainer?.Sid ?? nint.Zero);
            try
            {
                job.AssignProcess(worker.ProcessHandle);
            }
            catch
            {
                worker.Terminate(exitCode: 1);
                throw;
            }

            process = Process.GetProcessById(checked((int)worker.ProcessId));
            standardErrorTask = DrainAsync(
                worker.StandardError,
                request.MaximumCapturedStandardErrorCharacters);
            standardOutputTask = DrainAsync(worker.StandardOutput, maximumCharacters: 0);

            worker.Resume();
            using CancellationTokenSource timeoutCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCancellation.CancelAfter(request.Timeout);

            try
            {
                Task processExitTask = process.WaitForExitAsync(timeoutCancellation.Token);
                while (!processExitTask.IsCompleted)
                {
                    Task completed = await Task.WhenAny(
                        processExitTask,
                        Task.Delay(OutputPollInterval, CancellationToken.None)).ConfigureAwait(false);
                    if (completed != processExitTask)
                    {
                        ThrowIfOutputBudgetExceeded(request, stagingBaseline);
                    }
                }

                await processExitTask.ConfigureAwait(false);
                ThrowIfOutputBudgetExceeded(request, stagingBaseline);
            }
            catch (WorkerOutputLimitExceededException)
            {
                TerminateAndDisposeJob(job, exitCode: 126);
                job = null;
                await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
                throw;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                TerminateAndDisposeJob(job, exitCode: 124);
                job = null;
                await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
                throw new TimeoutException("Converty worker exceeded the configured execution timeout.");
            }
            catch (OperationCanceledException)
            {
                TerminateAndDisposeJob(job, exitCode: 125);
                job = null;
                await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
                throw;
            }

            int exitCode = process.ExitCode;

            // Closing a kill-on-close job after the worker exits removes any descendant
            // that attempted to outlive the single conversion worker.
            job.Dispose();
            job = null;

            string standardError = await standardErrorTask.ConfigureAwait(false);
            _ = await standardOutputTask.ConfigureAwait(false);
            return new WorkerProcessResult(exitCode, standardError);
        }
        finally
        {
            job?.Dispose();
            process?.Dispose();
            worker?.Dispose();
            DisposeIsolation(stagingGrant, applicationGrant, appContainer);
        }
    }

    // Retained as a non-launching projection for structured-argument regression tests
    // and repository guardrails. ExecuteAsync uses the suspended native launch path.
    public static ProcessStartInfo CreateStartInfo(WorkerProcessLaunchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = request.WorkingDirectory,
        };
        foreach (string argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static void Validate(WorkerProcessLaunchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExecutablePath) ||
            !Path.IsPathFullyQualified(request.ExecutablePath))
        {
            throw new ArgumentException("Worker executable path must be fully qualified.", nameof(request));
        }
        if (string.IsNullOrWhiteSpace(request.WorkingDirectory) ||
            !Path.IsPathFullyQualified(request.WorkingDirectory))
        {
            throw new ArgumentException("Worker working directory must be fully qualified.", nameof(request));
        }
        ArgumentNullException.ThrowIfNull(request.Arguments);
        if (request.Arguments.Count > MaximumArgumentCount)
        {
            throw new ArgumentException("Worker argument count exceeds the bounded launch surface.", nameof(request));
        }
        foreach (string argument in request.Arguments)
        {
            if (argument is null || argument.Length > MaximumArgumentCharacters)
            {
                throw new ArgumentException("Worker argument exceeds the bounded launch surface.", nameof(request));
            }
        }
        if (!Enum.IsDefined(request.IsolationLevel))
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }
        ArgumentNullException.ThrowIfNull(request.ResourceLimits);
        ArgumentNullException.ThrowIfNull(request.FileSystemScope);
        if (request.IsolationLevel == WorkerIsolationLevel.Strict)
        {
            string? executableDirectory = Path.GetDirectoryName(Path.GetFullPath(request.ExecutablePath));
            string workingDirectory = Path.GetFullPath(request.WorkingDirectory);
            if (executableDirectory is null ||
                !string.Equals(executableDirectory, workingDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Strict worker working directory must be the executable application directory.",
                    nameof(request));
            }
        }
        if (request.Timeout <= TimeSpan.Zero || request.Timeout > MaximumExecutionTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }
        if (request.MaximumCapturedStandardErrorCharacters is < 0 or > MaximumCapturedErrorCharacters)
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }
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

    private static Dictionary<string, long> CaptureFileLengths(string root)
    {
        var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(Path.GetFullPath(root));

        while (pendingDirectories.Count > 0)
        {
            string directory = pendingDirectories.Pop();
            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(directory).ToArray();
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (string entry in entries)
            {
                FileAttributes attributes;
                try
                {
                    attributes = File.GetAttributes(entry);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                catch (DirectoryNotFoundException)
                {
                    continue;
                }

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("Worker staging output monitor must not cross a reparse point.");
                }
                if ((attributes & FileAttributes.Directory) != 0)
                {
                    pendingDirectories.Push(entry);
                    continue;
                }

                try
                {
                    result[Path.GetFullPath(entry)] = new FileInfo(entry).Length;
                }
                catch (FileNotFoundException)
                {
                    // The worker removed the file between enumeration and length sampling.
                }
            }
        }

        return result;
    }

    private static void ThrowIfOutputBudgetExceeded(
        WorkerProcessLaunchRequest request,
        IReadOnlyDictionary<string, long> stagingBaseline)
    {
        IReadOnlyDictionary<string, long> current =
            CaptureFileLengths(request.FileSystemScope.WritableDirectory);
        long growth = 0;
        foreach ((string path, long currentLength) in current)
        {
            long baselineLength = stagingBaseline.TryGetValue(path, out long initialLength)
                ? initialLength
                : 0;
            if (currentLength <= baselineLength)
            {
                continue;
            }

            growth = checked(growth + (currentLength - baselineLength));
            if (growth > request.ResourceLimits.MaximumOutputBytes)
            {
                throw new WorkerOutputLimitExceededException(
                    request.ResourceLimits.MaximumOutputBytes,
                    growth);
            }
        }
    }

    private static void TerminateAndDisposeJob(WindowsJobObject job, uint exitCode)
    {
        try
        {
            job.Terminate(exitCode);
        }
        catch (Win32Exception)
        {
            // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE remains the fail-closed backstop.
        }
        finally
        {
            job.Dispose();
        }
    }

    private static void DisposeIsolation(
        WindowsAclGrant? stagingGrant,
        WindowsAclGrant? applicationGrant,
        WindowsAppContainerProfile? appContainer)
    {
        try
        {
            stagingGrant?.Dispose();
        }
        finally
        {
            try
            {
                applicationGrant?.Dispose();
            }
            finally
            {
                appContainer?.Dispose();
            }
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
}
