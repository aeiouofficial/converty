using System.Diagnostics;
using System.Text;

namespace Converty.Provider.FFmpeg;

public sealed record FfprobeExecutionResult(int ExitCode, string StandardOutput, string StandardError);

public sealed class FfprobeOutputLimitExceededException(string streamName, int maximumBytes, long observedBytes)
    : IOException($"FFprobe {streamName} exceeded the bounded output budget of {maximumBytes} bytes (observed at least {observedBytes}).")
{
    public string StreamName { get; } = streamName;
    public int MaximumBytes { get; } = maximumBytes;
    public long ObservedBytes { get; } = observedBytes;
}

public static class FfprobeProcessLauncher
{
    public const int MaximumCapturedStandardOutputBytes = 256 * 1024;
    public const int MaximumCapturedStandardErrorBytes = 64 * 1024;
    public static readonly TimeSpan MaximumExecutionTimeout = TimeSpan.FromMinutes(2);

    public static ProcessStartInfo CreateStartInfo(string ffprobePath, string inputPath)
    {
        string trustedExecutable = ValidateExecutable(ffprobePath);
        string trustedInput = ValidateInput(inputPath);
        string workingDirectory = Path.GetDirectoryName(trustedExecutable) ??
            throw new InvalidOperationException("FFprobe executable path requires a parent directory.");

        var startInfo = new ProcessStartInfo
        {
            FileName = trustedExecutable,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory,
        };

        foreach (string argument in new[]
        {
            "-v", "error",
            "-show_format",
            "-show_streams",
            "-show_chapters",
            "-of", "json",
            "-protocol_whitelist", "file",
            trustedInput,
        })
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    public static async Task<FfprobeExecutionResult> ExecuteAsync(
        string ffprobePath,
        string inputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero || timeout > MaximumExecutionTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        var process = new Process
        {
            StartInfo = CreateStartInfo(ffprobePath, inputPath),
            EnableRaisingEvents = true,
        };

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("FFprobe process did not start.");
            }

            Task<string> standardOutputTask = CaptureUtf8Async(
                process.StandardOutput.BaseStream,
                MaximumCapturedStandardOutputBytes,
                "stdout");
            Task<string> standardErrorTask = CaptureUtf8Async(
                process.StandardError.BaseStream,
                MaximumCapturedStandardErrorBytes,
                "stderr");

            using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCancellation.CancelAfter(timeout);
            Task exitTask = process.WaitForExitAsync(timeoutCancellation.Token);

            try
            {
                bool outputObserved = false;
                bool errorObserved = false;
                while (!exitTask.IsCompleted)
                {
                    Task completed = await Task.WhenAny(
                        exitTask,
                        outputObserved ? Task.Delay(Timeout.InfiniteTimeSpan, CancellationToken.None) : standardOutputTask,
                        errorObserved ? Task.Delay(Timeout.InfiniteTimeSpan, CancellationToken.None) : standardErrorTask).ConfigureAwait(false);

                    if (completed == standardOutputTask)
                    {
                        _ = await standardOutputTask.ConfigureAwait(false);
                        outputObserved = true;
                    }
                    else if (completed == standardErrorTask)
                    {
                        _ = await standardErrorTask.ConfigureAwait(false);
                        errorObserved = true;
                    }
                }

                await exitTask.ConfigureAwait(false);
                string standardOutput = await standardOutputTask.ConfigureAwait(false);
                string standardError = await standardErrorTask.ConfigureAwait(false);
                return new FfprobeExecutionResult(process.ExitCode, standardOutput, standardError);
            }
            catch (FfprobeOutputLimitExceededException)
            {
                KillProcessTree(process);
                await IgnoreCaptureFailuresAsync(standardOutputTask, standardErrorTask).ConfigureAwait(false);
                throw;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                KillProcessTree(process);
                await IgnoreCaptureFailuresAsync(standardOutputTask, standardErrorTask).ConfigureAwait(false);
                throw new TimeoutException("FFprobe exceeded the configured execution timeout.");
            }
            catch (OperationCanceledException)
            {
                KillProcessTree(process);
                await IgnoreCaptureFailuresAsync(standardOutputTask, standardErrorTask).ConfigureAwait(false);
                throw;
            }
        }
        finally
        {
            process.Dispose();
        }
    }

    private static string ValidateExecutable(string ffprobePath)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !Path.IsPathFullyQualified(ffprobePath))
        {
            throw new ArgumentException("FFprobe executable path must be fully qualified.", nameof(ffprobePath));
        }

        string fullPath = Path.GetFullPath(ffprobePath);
        if (!string.Equals(Path.GetFileName(fullPath), TrustedFfprobePath.ExecutableFileName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only the fixed ffprobe.exe executable name is accepted.", nameof(ffprobePath));
        }
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Trusted ffprobe.exe is missing.", fullPath);
        }
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Trusted ffprobe.exe must not be a reparse point.");
        }

        return fullPath;
    }

    private static string ValidateInput(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || !Path.IsPathFullyQualified(inputPath))
        {
            throw new ArgumentException("Probe input path must be fully qualified.", nameof(inputPath));
        }

        string fullPath = Path.GetFullPath(inputPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Probe input file is missing.", fullPath);
        }
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException("Probe input must not be a reparse point.");
        }

        return fullPath;
    }

    private static async Task<string> CaptureUtf8Async(Stream stream, int maximumBytes, string streamName)
    {
        byte[] buffer = new byte[4096];
        using var captured = new MemoryStream(Math.Min(maximumBytes, buffer.Length));
        long observed = 0;
        while (true)
        {
            int read = await stream.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            observed = checked(observed + read);
            if (observed > maximumBytes)
            {
                throw new FfprobeOutputLimitExceededException(streamName, maximumBytes, observed);
            }

            captured.Write(buffer, 0, read);
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
            .GetString(captured.GetBuffer(), 0, checked((int)captured.Length));
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task IgnoreCaptureFailuresAsync(params Task<string>[] tasks)
    {
        foreach (Task<string> task in tasks)
        {
            try
            {
                _ = await task.ConfigureAwait(false);
            }
            catch (Exception error) when (error is IOException or DecoderFallbackException or ObjectDisposedException)
            {
            }
        }
    }
}
