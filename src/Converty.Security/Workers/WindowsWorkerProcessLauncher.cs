using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Converty.Security.Workers;

public sealed class WindowsWorkerProcessLauncher : IWorkerProcessLauncher
{
    public static readonly TimeSpan MaximumExecutionTimeout = TimeSpan.FromMinutes(30);
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

        if (request.IsolationLevel == WorkerIsolationLevel.Strict)
        {
            throw new NotSupportedException(
                "Strict worker isolation is not enabled until its no-network and staging-write policy is qualified.");
        }

        WindowsJobObject? job = null;
        WindowsSuspendedProcess? worker = null;
        Process? process = null;
        Task<string>? standardErrorTask = null;
        Task<string>? standardOutputTask = null;

        try
        {
            job = WindowsJobObject.Create(request.ResourceLimits);
            worker = WindowsSuspendedProcess.Create(request);
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
                await process.WaitForExitAsync(timeoutCancellation.Token).ConfigureAwait(false);
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
