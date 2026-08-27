using System.Net;
using System.Net.Sockets;
using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class WindowsWorkerStrictIsolationCanaryTests
{
    [Fact]
    public async Task StrictWorkerCanWriteOnlyInsideGrantedStagingScope()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        CancellationToken testCancellation = TestContext.Current.CancellationToken;
        string canaryExecutable = ResolveCanaryExecutable();
        string appDirectory = Path.GetDirectoryName(canaryExecutable) ??
            throw new InvalidOperationException("Canary executable requires an application directory.");
        string root = Path.Combine(Path.GetTempPath(), $"ConvertyStrictFs-{Guid.NewGuid():N}");
        string stagingDirectory = Path.Combine(root, "staging");
        string outsideDirectory = Path.Combine(root, "outside");
        Directory.CreateDirectory(stagingDirectory);
        Directory.CreateDirectory(outsideDirectory);
        string insidePath = Path.Combine(stagingDirectory, "inside.txt");
        string outsidePath = Path.Combine(outsideDirectory, "outside.txt");

        try
        {
            var launcher = new WindowsWorkerProcessLauncher();
            WorkerProcessResult inside = await launcher.ExecuteAsync(
                CreateStrictRequest(canaryExecutable, appDirectory, stagingDirectory, ["--write-file", insidePath]),
                testCancellation);

            Assert.Equal(0, inside.ExitCode);
            Assert.True(File.Exists(insidePath));

            WorkerProcessResult outside = await launcher.ExecuteAsync(
                CreateStrictRequest(canaryExecutable, appDirectory, stagingDirectory, ["--write-file", outsidePath]),
                testCancellation);

            Assert.NotEqual(0, outside.ExitCode);
            Assert.False(File.Exists(outsidePath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task StrictWorkerCannotOpenLoopbackNetworkConnection()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        CancellationToken testCancellation = TestContext.Current.CancellationToken;
        string canaryExecutable = ResolveCanaryExecutable();
        string appDirectory = Path.GetDirectoryName(canaryExecutable) ??
            throw new InvalidOperationException("Canary executable requires an application directory.");
        string stagingDirectory = Path.Combine(Path.GetTempPath(), $"ConvertyStrictNet-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingDirectory);

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        try
        {
            var launcher = new WindowsWorkerProcessLauncher();
            WorkerProcessResult result = await launcher.ExecuteAsync(
                CreateStrictRequest(
                    canaryExecutable,
                    appDirectory,
                    stagingDirectory,
                    ["--connect-loopback", port.ToString(System.Globalization.CultureInfo.InvariantCulture)]),
                testCancellation);

            Assert.NotEqual(0, result.ExitCode);

            using var acceptTimeout = CancellationTokenSource.CreateLinkedTokenSource(testCancellation);
            acceptTimeout.CancelAfter(TimeSpan.FromMilliseconds(500));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await listener.AcceptTcpClientAsync(acceptTimeout.Token));
        }
        finally
        {
            listener.Stop();
            Directory.Delete(stagingDirectory, recursive: true);
        }
    }

    private static WorkerProcessLaunchRequest CreateStrictRequest(
        string executable,
        string appDirectory,
        string stagingDirectory,
        IReadOnlyList<string> arguments) =>
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
            new WorkerFileSystemScope(stagingDirectory),
            TimeSpan.FromSeconds(15),
            MaximumCapturedStandardErrorCharacters: 4096);

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
}
