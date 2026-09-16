using System.Diagnostics;
using System.Security.Cryptography;
using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class ActualEngineDescendantIsolationTests
{
    [Fact]
    public async Task ActualFfprobeDescendantCanReadOnlyGrantedInput()
    {
        if (!TryResolvePackagedEngines(out EnginePaths? packaged))
        {
            return;
        }

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string root = CreateTempDirectory("ConvertyActualProbeScope");
        try
        {
            StagedApplication app = StageCanaryApplication(root, packaged);
            string fixtureDirectory = Path.Combine(root, "fixtures");
            Directory.CreateDirectory(fixtureDirectory);
            string grantedInput = Path.Combine(fixtureDirectory, "granted.mp4");
            string prohibitedInput = Path.Combine(fixtureDirectory, "prohibited.mp4");
            await CreateMp4FixtureAsync(packaged.Ffmpeg, grantedInput, cancellationToken);
            File.Copy(grantedInput, prohibitedInput);
            string grantedHash = ComputeSha256(grantedInput);
            string prohibitedHash = ComputeSha256(prohibitedInput);

            var launcher = new WindowsWorkerProcessLauncher();
            WorkerFileSystemScope scope = WorkerFileSystemScope.ForReadOnlyFile(grantedInput);
            WorkerProcessResult allowed = await launcher.ExecuteAsync(
                CreateStrictRequest(
                    app.Canary,
                    app.Root,
                    scope,
                    ["--spawn-ffprobe-read", app.Ffprobe, grantedInput]),
                cancellationToken);
            Assert.Equal(0, allowed.ExitCode);

            WorkerProcessResult denied = await launcher.ExecuteAsync(
                CreateStrictRequest(
                    app.Canary,
                    app.Root,
                    scope,
                    ["--spawn-ffprobe-read", app.Ffprobe, prohibitedInput]),
                cancellationToken);
            Assert.NotEqual(0, denied.ExitCode);
            Assert.Equal(grantedHash, ComputeSha256(grantedInput));
            Assert.Equal(prohibitedHash, ComputeSha256(prohibitedInput));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ActualFfmpegDescendantCanWriteOnlyInsideGrantedStaging()
    {
        if (!TryResolvePackagedEngines(out EnginePaths? packaged))
        {
            return;
        }

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        string root = CreateTempDirectory("ConvertyActualEngineScope");
        try
        {
            StagedApplication app = StageCanaryApplication(root, packaged);
            string stagingDirectory = Path.Combine(root, "staging");
            string outsideDirectory = Path.Combine(root, "outside");
            Directory.CreateDirectory(stagingDirectory);
            Directory.CreateDirectory(outsideDirectory);
            string insideOutput = Path.Combine(stagingDirectory, "inside.wav");
            string outsideOutput = Path.Combine(outsideDirectory, "outside.wav");

            var launcher = new WindowsWorkerProcessLauncher();
            WorkerFileSystemScope scope = new(stagingDirectory);
            WorkerProcessResult allowed = await launcher.ExecuteAsync(
                CreateStrictRequest(
                    app.Canary,
                    app.Root,
                    scope,
                    ["--spawn-ffmpeg-write-wave", app.Ffmpeg, insideOutput]),
                cancellationToken);
            Assert.Equal(0, allowed.ExitCode);
            Assert.True(File.Exists(insideOutput));
            Assert.True(new FileInfo(insideOutput).Length > 0);

            WorkerProcessResult denied = await launcher.ExecuteAsync(
                CreateStrictRequest(
                    app.Canary,
                    app.Root,
                    scope,
                    ["--spawn-ffmpeg-write-wave", app.Ffmpeg, outsideOutput]),
                cancellationToken);
            Assert.NotEqual(0, denied.ExitCode);
            Assert.False(File.Exists(outsideOutput));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ActualFfmpegDescendantDiesWithWorkerJobOnCancellation()
    {
        if (!TryResolvePackagedEngines(out EnginePaths? packaged))
        {
            return;
        }

        CancellationToken testCancellation = TestContext.Current.CancellationToken;
        string root = CreateTempDirectory("ConvertyActualEngineJob");
        try
        {
            StagedApplication app = StageCanaryApplication(root, packaged);
            string stagingDirectory = Path.Combine(root, "staging");
            Directory.CreateDirectory(stagingDirectory);
            string pidPath = Path.Combine(stagingDirectory, "ffmpeg.pid");
            var launcher = new WindowsWorkerProcessLauncher();
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(testCancellation);

            Task<WorkerProcessResult> execution = launcher.ExecuteAsync(
                CreateStrictRequest(
                    app.Canary,
                    app.Root,
                    new WorkerFileSystemScope(stagingDirectory),
                    ["--spawn-ffmpeg-hold", app.Ffmpeg, pidPath],
                    timeout: TimeSpan.FromSeconds(15)),
                cancellation.Token);

            int childPid = await WaitForPidAsync(pidPath, TimeSpan.FromSeconds(5), testCancellation);
            using (Process child = Process.GetProcessById(childPid))
            {
                Assert.False(child.HasExited);
            }

            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await execution);
            await AssertProcessExitedAsync(childPid, TimeSpan.FromSeconds(5), testCancellation);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static WorkerProcessLaunchRequest CreateStrictRequest(
        string executable,
        string appDirectory,
        WorkerFileSystemScope scope,
        IReadOnlyList<string> arguments,
        TimeSpan? timeout = null) =>
        new(
            executable,
            appDirectory,
            arguments,
            WorkerIsolationLevel.Strict,
            new WorkerResourceLimits(
                maximumActiveProcesses: 2,
                maximumProcessMemoryBytes: 512L * 1024 * 1024,
                maximumJobMemoryBytes: 768L * 1024 * 1024,
                maximumCpuRatePercent: 100),
            scope,
            timeout ?? TimeSpan.FromSeconds(15),
            MaximumCapturedStandardErrorCharacters: 16 * 1024);

    private static bool TryResolvePackagedEngines(out EnginePaths? engines)
    {
        engines = null;
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        string repositoryRoot = ResolveRepositoryRoot();
        string engineDirectory = Path.Combine(repositoryRoot, "artifacts", "dev-package-layout", "tools", "ffmpeg");
        string ffmpeg = Path.Combine(engineDirectory, "ffmpeg.exe");
        string ffprobe = Path.Combine(engineDirectory, "ffprobe.exe");
        if (!File.Exists(ffmpeg) || !File.Exists(ffprobe))
        {
            return false;
        }

        engines = new EnginePaths(ffmpeg, ffprobe);
        return true;
    }

    private static StagedApplication StageCanaryApplication(string root, EnginePaths packaged)
    {
        string applicationRoot = Path.Combine(root, "application");
        Directory.CreateDirectory(applicationRoot);
        string sourceCanary = ResolveCanaryExecutable();
        string sourceDirectory = Path.GetDirectoryName(sourceCanary) ??
            throw new InvalidOperationException("Canary executable requires a parent directory.");
        foreach (string source in Directory.EnumerateFiles(sourceDirectory, "Converty.WorkerCanary.*"))
        {
            File.Copy(source, Path.Combine(applicationRoot, Path.GetFileName(source)));
        }

        string stagedCanary = Path.Combine(applicationRoot, "Converty.WorkerCanary.exe");
        string engineDirectory = Path.Combine(applicationRoot, "tools", "ffmpeg");
        Directory.CreateDirectory(engineDirectory);
        string stagedFfmpeg = Path.Combine(engineDirectory, "ffmpeg.exe");
        string stagedFfprobe = Path.Combine(engineDirectory, "ffprobe.exe");
        File.Copy(packaged.Ffmpeg, stagedFfmpeg);
        File.Copy(packaged.Ffprobe, stagedFfprobe);
        Assert.Equal(ComputeSha256(packaged.Ffmpeg), ComputeSha256(stagedFfmpeg));
        Assert.Equal(ComputeSha256(packaged.Ffprobe), ComputeSha256(stagedFfprobe));
        return new StagedApplication(applicationRoot, stagedCanary, stagedFfmpeg, stagedFfprobe);
    }

    private static async Task CreateMp4FixtureAsync(string ffmpeg, string outputPath, CancellationToken cancellationToken)
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
        foreach (string argument in new[]
        {
            "-hide_banner", "-loglevel", "error",
            "-f", "lavfi", "-i", "testsrc2=size=32x24:rate=2",
            "-frames:v", "2", "-c:v", "libx264", "-preset", "ultrafast", "-pix_fmt", "yuv420p",
            "-f", "mp4", "-y", outputPath,
        })
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start());
        Task<string> stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        await process.WaitForExitAsync(timeout.Token);
        _ = await stdout;
        string error = await stderr;
        Assert.True(process.ExitCode == 0, $"Could not create actual-engine MP4 fixture: {error}");
        Assert.True(File.Exists(outputPath));
    }

    private static async Task<int> WaitForPidAsync(string pidPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        DateTime deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(pidPath))
            {
                string text = await File.ReadAllTextAsync(pidPath, cancellationToken);
                if (int.TryParse(text, out int pid))
                {
                    return pid;
                }
            }
            await Task.Delay(25, cancellationToken);
        }
        throw new TimeoutException("Actual ffmpeg descendant did not publish its PID before the test deadline.");
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
        Assert.Fail($"Actual ffmpeg descendant pid={pid} survived worker Job cancellation.");
    }

    private static string ComputeSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string ResolveCanaryExecutable()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        DirectoryInfo frameworkDirectory = new(AppContext.BaseDirectory);
        string configuration = frameworkDirectory.Parent?.Name ??
            throw new InvalidOperationException("Test configuration directory could not be resolved.");
        string path = Path.Combine(repositoryRoot, "tests", "Converty.WorkerCanary", "bin", configuration, "net10.0", "Converty.WorkerCanary.exe");
        Assert.True(File.Exists(path), $"Strict isolation canary executable is missing: {path}");
        return path;
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

    private sealed record EnginePaths(string Ffmpeg, string Ffprobe);
    private sealed record StagedApplication(string Root, string Canary, string Ffmpeg, string Ffprobe);
}
