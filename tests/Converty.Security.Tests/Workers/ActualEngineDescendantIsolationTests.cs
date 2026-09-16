using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class ActualEngineDescendantIsolationTests
{
    private const uint TokenQuery = 0x0008;
    private const int TokenIsAppContainer = 29;

    [Fact]
    public async Task ActualPackagedProbeWorkerUsesBundledFfprobeUnderStrictReadOnlyScope()
    {
        if (!TryResolvePackagedRuntime(out PackagedRuntime runtime))
        {
            return;
        }

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string root = CreateTempDirectory("ConvertyActualProbeWorker");
        try
        {
            string input = Path.Combine(root, "probe.mp4");
            await CreateVideoFixtureAsync(runtime.Ffmpeg, input, "mp4", cancellationToken);
            string sourceHash = ComputeSha256(input);
            var launcher = new WindowsWorkerProcessLauncher();

            WorkerProcessResult result = await launcher.ExecuteAsync(
                CreateStrictRequest(
                    runtime.ProbeWorker,
                    runtime.Layout,
                    WorkerFileSystemScope.ForReadOnlyFile(input),
                    ["--input", input],
                    maximumCapturedStandardOutputBytes: 256 * 1024),
                cancellationToken);

            Assert.Equal(0, result.ExitCode);
            using JsonDocument probeResult = JsonDocument.Parse(result.StandardOutput);
            Assert.Equal(1, probeResult.RootElement.GetProperty("schemaVersion").GetInt32());
            Assert.Equal("success", probeResult.RootElement.GetProperty("status").GetString());
            Assert.Equal("none", probeResult.RootElement.GetProperty("failureReason").GetString());
            Assert.True(probeResult.RootElement.TryGetProperty("facts", out JsonElement facts));
            Assert.Equal(JsonValueKind.Object, facts.ValueKind);
            Assert.Equal(sourceHash, ComputeSha256(input));
            AssertNoRunningProcessAtPath(runtime.Ffprobe);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ActualPackagedEngineWorkerUsesBundledFfmpegUnderStrictWritableScope()
    {
        if (!TryResolvePackagedRuntime(out PackagedRuntime runtime))
        {
            return;
        }

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string root = CreateTempDirectory("ConvertyActualEngineWorker");
        try
        {
            string staging = Path.Combine(root, "staging");
            Directory.CreateDirectory(staging);
            string input = Path.Combine(staging, "source.avi");
            string output = Path.Combine(staging, "result.webm");
            await CreateVideoFixtureAsync(runtime.Ffmpeg, input, "avi", cancellationToken);
            string sourceHash = ComputeSha256(input);
            var launcher = new WindowsWorkerProcessLauncher();

            WorkerProcessResult result = await launcher.ExecuteAsync(
                CreateStrictRequest(
                    runtime.EngineWorker,
                    runtime.Layout,
                    new WorkerFileSystemScope(staging),
                    ["--preset", "video.webm.vp9", "--mode", "transcode", "--input", input, "--output", output]),
                cancellationToken);

            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(output));
            Assert.True(new FileInfo(output).Length > 0);
            Assert.Equal(sourceHash, ComputeSha256(input));
            AssertNoRunningProcessAtPath(runtime.Ffmpeg);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ActualPackagedFfmpegDescendantIsAppContainerAndDiesWithWorkerJobOnCancellation()
    {
        if (!TryResolvePackagedRuntime(out PackagedRuntime runtime))
        {
            return;
        }

        CancellationToken testCancellation = TestContext.Current.CancellationToken;
        string root = CreateTempDirectory("ConvertyActualEngineCancellation");
        try
        {
            string staging = Path.Combine(root, "staging");
            Directory.CreateDirectory(staging);
            string input = Path.Combine(staging, "long-source.avi");
            string output = Path.Combine(staging, "result.webm");
            await CreateLongVideoFixtureAsync(runtime.Ffmpeg, input, testCancellation);
            var launcher = new WindowsWorkerProcessLauncher();
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(testCancellation);

            Task<WorkerProcessResult> execution = launcher.ExecuteAsync(
                CreateStrictRequest(
                    runtime.EngineWorker,
                    runtime.Layout,
                    new WorkerFileSystemScope(staging),
                    ["--preset", "video.webm.vp9", "--mode", "transcode", "--input", input, "--output", output],
                    timeout: TimeSpan.FromSeconds(20)),
                cancellation.Token);

            int ffmpegPid = await WaitForProcessAtPathAsync(runtime.Ffmpeg, TimeSpan.FromSeconds(8), testCancellation);
            using (Process ffmpeg = Process.GetProcessById(ffmpegPid))
            {
                Assert.False(ffmpeg.HasExited);
                Assert.True(IsAppContainerProcess(ffmpeg), $"Actual packaged ffmpeg pid={ffmpegPid} did not inherit the strict AppContainer token.");
            }

            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await execution);
            await AssertProcessExitedAsync(ffmpegPid, TimeSpan.FromSeconds(5), testCancellation);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static WorkerProcessLaunchRequest CreateStrictRequest(
        string executable,
        string applicationDirectory,
        WorkerFileSystemScope scope,
        IReadOnlyList<string> arguments,
        TimeSpan? timeout = null,
        int maximumCapturedStandardOutputBytes = 0) =>
        new(
            executable,
            applicationDirectory,
            arguments,
            WorkerIsolationLevel.Strict,
            new WorkerResourceLimits(
                maximumActiveProcesses: 2,
                maximumProcessMemoryBytes: 768L * 1024 * 1024,
                maximumJobMemoryBytes: 1024L * 1024 * 1024,
                maximumCpuRatePercent: 100),
            scope,
            timeout ?? TimeSpan.FromSeconds(20),
            MaximumCapturedStandardErrorCharacters: 64 * 1024,
            maximumCapturedStandardOutputBytes);

    private static bool TryResolvePackagedRuntime(out PackagedRuntime runtime)
    {
        runtime = null!;
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        string repositoryRoot = ResolveRepositoryRoot();
        string layout = Path.Combine(repositoryRoot, "artifacts", "dev-package-layout");
        string probeWorker = Path.Combine(layout, "Converty.ProbeWorker.exe");
        string engineWorker = Path.Combine(layout, "Converty.EngineWorker.exe");
        string engineDirectory = Path.Combine(layout, "tools", "ffmpeg");
        string ffmpeg = Path.Combine(engineDirectory, "ffmpeg.exe");
        string ffprobe = Path.Combine(engineDirectory, "ffprobe.exe");
        if (!File.Exists(probeWorker) || !File.Exists(engineWorker) || !File.Exists(ffmpeg) || !File.Exists(ffprobe))
        {
            return false;
        }

        runtime = new PackagedRuntime(layout, probeWorker, engineWorker, ffmpeg, ffprobe);
        return true;
    }

    private static async Task CreateVideoFixtureAsync(
        string ffmpeg,
        string outputPath,
        string format,
        CancellationToken cancellationToken)
    {
        string[] codecArguments = format switch
        {
            "mp4" => ["-c:v", "libx264", "-preset", "ultrafast", "-pix_fmt", "yuv420p", "-f", "mp4"],
            "avi" => ["-c:v", "mpeg2video", "-q:v", "5", "-pix_fmt", "yuv420p", "-f", "avi"],
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };

        await RunFixtureFfmpegAsync(
            ffmpeg,
            outputPath,
            [
                "-hide_banner", "-loglevel", "error",
                "-f", "lavfi", "-i", "testsrc2=size=64x48:rate=10",
                "-t", "0.5",
                .. codecArguments,
                "-y", outputPath,
            ],
            cancellationToken);
    }

    private static Task CreateLongVideoFixtureAsync(string ffmpeg, string outputPath, CancellationToken cancellationToken) =>
        RunFixtureFfmpegAsync(
            ffmpeg,
            outputPath,
            [
                "-hide_banner", "-loglevel", "error",
                "-f", "lavfi", "-i", "testsrc2=size=640x360:rate=30",
                "-t", "20",
                "-c:v", "mpeg2video", "-q:v", "2", "-pix_fmt", "yuv420p",
                "-f", "avi",
                "-y", outputPath,
            ],
            cancellationToken);

    private static async Task RunFixtureFfmpegAsync(
        string ffmpeg,
        string outputPath,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpeg,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(ffmpeg) ?? throw new InvalidOperationException("FFmpeg requires a parent directory."),
        };
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start());
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(timeout.Token);
        _ = await stdout;
        string error = await stderr;
        Assert.True(process.ExitCode == 0, $"Could not create engine qualification fixture '{outputPath}': {error}");
        Assert.True(File.Exists(outputPath));
    }

    private static async Task<int> WaitForProcessAtPathAsync(
        string executablePath,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        DateTime deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int? pid = FindProcessAtPath(executablePath);
            if (pid.HasValue)
            {
                return pid.Value;
            }
            await Task.Delay(25, cancellationToken);
        }
        throw new TimeoutException($"Actual packaged engine process was not observed: {executablePath}");
    }

    private static void AssertNoRunningProcessAtPath(string executablePath) =>
        Assert.Null(FindProcessAtPath(executablePath));

    private static int? FindProcessAtPath(string executablePath)
    {
        string processName = Path.GetFileNameWithoutExtension(executablePath);
        foreach (Process process in Process.GetProcessesByName(processName))
        {
            using (process)
            {
                try
                {
                    string? candidate = process.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(candidate) &&
                        string.Equals(Path.GetFullPath(candidate), Path.GetFullPath(executablePath), StringComparison.OrdinalIgnoreCase))
                    {
                        return process.Id;
                    }
                }
                catch (InvalidOperationException)
                {
                }
                catch (Win32Exception)
                {
                }
            }
        }
        return null;
    }

    private static async Task AssertProcessExitedAsync(int pid, TimeSpan timeout, CancellationToken cancellationToken)
    {
        DateTime deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using Process process = Process.GetProcessById(pid);
                if (process.HasExited)
                {
                    return;
                }
            }
            catch (ArgumentException)
            {
                return;
            }
            await Task.Delay(25, cancellationToken);
        }
        Assert.Fail($"Actual packaged ffmpeg descendant pid={pid} survived strict worker Job cancellation.");
    }

    private static bool IsAppContainerProcess(Process process)
    {
        if (!OpenProcessToken(process.Handle, TokenQuery, out nint token))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not open actual engine process token.");
        }

        try
        {
            int isAppContainer = 0;
            if (!GetTokenInformation(token, TokenIsAppContainer, out isAppContainer, sizeof(int), out _))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not inspect actual engine AppContainer token state.");
            }
            return isAppContainer != 0;
        }
        finally
        {
            _ = CloseHandle(token);
        }
    }

    private static string ComputeSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Converty.slnx")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        throw new InvalidOperationException("Repository root could not be resolved from the test output directory.");
    }

    private static string CreateTempDirectory(string prefix)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(nint processHandle, uint desiredAccess, out nint tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTokenInformation(
        nint tokenHandle,
        int tokenInformationClass,
        out int tokenInformation,
        int tokenInformationLength,
        out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);

    private sealed record PackagedRuntime(
        string Layout,
        string ProbeWorker,
        string EngineWorker,
        string Ffmpeg,
        string Ffprobe);
}
