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

        ProcessStartInfo startInfo = CreateStartInfo(request);
        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException("Converty worker process could not be started.");
        }

        Task<string> standardErrorTask = DrainAsync(
            process.StandardError,
            request.MaximumCapturedStandardErrorCharacters);
        Task<string> standardOutputTask = DrainAsync(process.StandardOutput, maximumCharacters: 0);
        using CancellationTokenSource timeoutCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(request.Timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
            throw new TimeoutException("Converty worker exceeded the configured execution timeout.");
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(process);
            await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
            throw;
        }

        string standardError = await standardErrorTask.ConfigureAwait(false);
        _ = await standardOutputTask.ConfigureAwait(false);
        return new WorkerProcessResult(process.ExitCode, standardError);
    }

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
}
