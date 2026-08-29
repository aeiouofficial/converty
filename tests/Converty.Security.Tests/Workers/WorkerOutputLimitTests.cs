using System.Globalization;
using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class WorkerOutputLimitTests
{
    [Fact]
    public void ConversionDefaultHasFiniteOutputBudget()
    {
        Assert.InRange(
            WorkerResourceLimits.ConversionDefault.MaximumOutputBytes,
            64L * 1024 * 1024,
            16L * 1024 * 1024 * 1024);
    }

    [Fact]
    public void ConstructorRejectsZeroOutputBudget()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorkerResourceLimits(
            maximumActiveProcesses: 2,
            maximumProcessMemoryBytes: 512L * 1024 * 1024,
            maximumJobMemoryBytes: 768L * 1024 * 1024,
            maximumOutputBytes: 0,
            maximumCpuRatePercent: 80));
    }

    [Fact]
    public async Task StrictWorkerIsTerminatedWhenStagingGrowthExceedsOutputBudget()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        CancellationToken testCancellation = TestContext.Current.CancellationToken;
        string canaryExecutable = ResolveCanaryExecutable();
        string appDirectory = Path.GetDirectoryName(canaryExecutable) ??
            throw new InvalidOperationException("Canary executable requires an application directory.");
        string stagingDirectory = Path.Combine(Path.GetTempPath(), $"ConvertyOutputLimit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingDirectory);
        string outputPath = Path.Combine(stagingDirectory, "growth.bin");
        const long maximumOutputBytes = 64L * 1024;
        const long targetOutputBytes = maximumOutputBytes + 4096;

        try
        {
            var request = new WorkerProcessLaunchRequest(
                canaryExecutable,
                appDirectory,
                [
                    "--write-slow-bytes-and-hold",
                    outputPath,
                    targetOutputBytes.ToString(CultureInfo.InvariantCulture),
                ],
                WorkerIsolationLevel.Strict,
                new WorkerResourceLimits(
                    maximumActiveProcesses: 2,
                    maximumProcessMemoryBytes: 512L * 1024 * 1024,
                    maximumJobMemoryBytes: 768L * 1024 * 1024,
                    maximumOutputBytes: maximumOutputBytes,
                    maximumCpuRatePercent: 100),
                new WorkerFileSystemScope(stagingDirectory),
                TimeSpan.FromSeconds(15),
                MaximumCapturedStandardErrorCharacters: 4096);

            var launcher = new WindowsWorkerProcessLauncher();
            WorkerOutputLimitExceededException? failure = null;
            WorkerProcessResult? unexpectedResult = null;
            try
            {
                unexpectedResult = await launcher.ExecuteAsync(request, testCancellation);
            }
            catch (WorkerOutputLimitExceededException ex)
            {
                failure = ex;
            }

            if (failure is null)
            {
                long stagedBytes = File.Exists(outputPath) ? new FileInfo(outputPath).Length : -1;
                string exitCode = unexpectedResult is null
                    ? "<none>"
                    : unexpectedResult.ExitCode.ToString(CultureInfo.InvariantCulture);
                Assert.Fail(
                    $"Expected output-budget termination, but worker returned exit code " +
                    $"{exitCode}; stagedBytes={stagedBytes}; " +
                    $"stderr={unexpectedResult?.StandardError ?? "<none>"}");
            }

            Assert.Equal(maximumOutputBytes, failure.MaximumOutputBytes);
            Assert.InRange(failure.ObservedOutputGrowthBytes, maximumOutputBytes + 1, targetOutputBytes);
            Assert.True(File.Exists(outputPath));
            Assert.InRange(new FileInfo(outputPath).Length, maximumOutputBytes + 1, targetOutputBytes);
        }
        finally
        {
            Directory.Delete(stagingDirectory, recursive: true);
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

        Assert.True(File.Exists(path), $"Worker output canary executable is missing: {path}");
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
