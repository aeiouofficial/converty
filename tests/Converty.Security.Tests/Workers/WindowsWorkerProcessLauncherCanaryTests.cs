using System.Diagnostics;
using System.Globalization;
using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class WindowsWorkerProcessLauncherCanaryTests
{
    [Fact]
    public async Task CompatibilityJobKillOnCloseTerminatesSpawnedDescendant()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        string canaryExecutable = ResolveCanaryExecutable();
        string canaryWorkingDirectory = Path.GetDirectoryName(canaryExecutable) ??
            throw new InvalidOperationException("Canary executable requires a working directory.");
        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"ConvertyWorkerCanary-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        string childPidPath = Path.Combine(temporaryDirectory, "child.pid");
        int? childPid = null;

        try
        {
            var launcher = new WindowsWorkerProcessLauncher();
            var request = new WorkerProcessLaunchRequest(
                canaryExecutable,
                canaryWorkingDirectory,
                ["--spawn-child-and-exit", childPidPath],
                WorkerIsolationLevel.Compatibility,
                new WorkerResourceLimits(
                    maximumActiveProcesses: 4,
                    maximumProcessMemoryBytes: 512L * 1024 * 1024,
                    maximumJobMemoryBytes: 768L * 1024 * 1024,
                    maximumCpuRatePercent: 100),
                TimeSpan.FromSeconds(15),
                maximumCapturedStandardErrorCharacters: 4096);

            WorkerProcessResult result = await launcher.ExecuteAsync(request);

            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(childPidPath), "Canary root process did not publish its child PID.");
            childPid = int.Parse(File.ReadAllText(childPidPath), CultureInfo.InvariantCulture);
            await AssertProcessExitedAsync(childPid.Value);
        }
        finally
        {
            if (childPid is int pid)
            {
                KillIfStillRunning(pid);
            }
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static string ResolveCanaryExecutable()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        DirectoryInfo frameworkDirectory = new(AppContext.BaseDirectory);
        string configuration = frameworkDirectory.Parent?.Name ??
            throw new InvalidOperationException("Test configuration directory could not be resolved.");
        string path = Path.Combine(
            repositoryRoot,
            "tests",
            "Converty.WorkerCanary",
            "bin",
            configuration,
            "net10.0",
            "Converty.WorkerCanary.exe");

        Assert.True(File.Exists(path), $"Worker containment canary executable is missing: {path}");
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

    private static async Task AssertProcessExitedAsync(int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await process.WaitForExitAsync(timeout.Token);
            Assert.True(process.HasExited, $"Canary descendant process {processId} survived Job Object close.");
        }
        catch (ArgumentException)
        {
            // Process already exited before the test opened a verification handle.
        }
    }

    private static void KillIfStillRunning(int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (ArgumentException)
        {
            // Process already exited.
        }
        catch (InvalidOperationException)
        {
            // Process exited between lookup and cleanup.
        }
    }
}
