using System.Diagnostics;
using System.Text;
using Converty.Core.Presets;

namespace Converty.Core.Execution;

public sealed class FfmpegProcessLauncher
{
    public static readonly TimeSpan MaximumExecutionTimeout = TimeSpan.FromMinutes(30);
    public const int MaximumCapturedErrorCharacters = 64 * 1024;

    public ProcessStartInfo CreateStartInfo(
        string ffmpegPath,
        ProductPresetDefinition preset,
        string inputPath,
        string outputPath)
    {
        if (string.IsNullOrWhiteSpace(ffmpegPath) || !Path.IsPathFullyQualified(ffmpegPath))
        {
            throw new ArgumentException("Trusted FFmpeg path must be fully qualified.", nameof(ffmpegPath));
        }

        ArgumentNullException.ThrowIfNull(preset);
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Input path is required.", nameof(inputPath));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path is required.", nameof(outputPath));
        }

        string? workingDirectory = Path.GetDirectoryName(ffmpegPath);
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new ArgumentException("Trusted FFmpeg path must have a parent directory.", nameof(ffmpegPath));
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = workingDirectory,
        };

        foreach (string argument in preset.BuildFfmpegArguments(inputPath, outputPath))
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    public async Task<FfmpegExecutionResult> ExecuteAsync(
        string ffmpegPath,
        ProductPresetDefinition preset,
        string inputPath,
        string outputPath,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ValidateTimeout(timeout);
        cancellationToken.ThrowIfCancellationRequested();

        ProcessStartInfo startInfo = CreateStartInfo(ffmpegPath, preset, inputPath, outputPath);
        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException("Bundled Converty ffmpeg.exe could not be started.");
        }

        Task<string> standardErrorTask = DrainAsync(process.StandardError, MaximumCapturedErrorCharacters);
        Task<string> standardOutputTask = DrainAsync(process.StandardOutput, maximumCharacters: 0);

        using CancellationTokenSource timeoutCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
            throw new TimeoutException("FFmpeg conversion exceeded the configured execution timeout.");
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(process);
            await IgnoreDrainFailuresAsync(standardErrorTask, standardOutputTask).ConfigureAwait(false);
            throw;
        }

        string standardError = await standardErrorTask.ConfigureAwait(false);
        _ = await standardOutputTask.ConfigureAwait(false);
        return new FfmpegExecutionResult(process.ExitCode, standardError);
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
        if (timeout <= TimeSpan.Zero || timeout > MaximumExecutionTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                $"FFmpeg execution timeout must be greater than zero and no more than {MaximumExecutionTimeout.TotalMinutes:0} minutes.");
        }
    }
}
