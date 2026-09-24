using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class WorkerStandardOutputLimitTests
{
    [Fact]
    public void LaunchRequestAndResultExposeIndependentStandardOutputContract()
    {
        PropertyInfo? requestProperty = typeof(WorkerProcessLaunchRequest)
            .GetProperty("MaximumCapturedStandardOutputBytes", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(requestProperty);
        Assert.Equal(typeof(int), requestProperty!.PropertyType);

        PropertyInfo? resultProperty = typeof(WorkerProcessResult)
            .GetProperty("StandardOutput", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(resultProperty);
        Assert.Equal(typeof(string), resultProperty!.PropertyType);
    }

    [Fact]
    public async Task StrictWorkerCapturesExactlyMaximumStandardOutputBytes()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        const int maximumBytes = 64;
        string canaryExecutable = ResolveCanaryExecutable();
        string appDirectory = Path.GetDirectoryName(canaryExecutable) ??
            throw new InvalidOperationException("Canary executable requires an application directory.");
        string stagingDirectory = Path.Combine(Path.GetTempPath(), $"ConvertyStdoutExact-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingDirectory);

        try
        {
            WorkerProcessLaunchRequest request = CreateRequest(
                canaryExecutable,
                appDirectory,
                ["--stdout-bytes", maximumBytes.ToString(CultureInfo.InvariantCulture)],
                new WorkerFileSystemScope(stagingDirectory),
                maximumBytes);

            WorkerProcessResult result = await new WindowsWorkerProcessLauncher()
                .ExecuteAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(0, result.ExitCode);
            string standardOutput = ReadStandardOutput(result);
            Assert.Equal(maximumBytes, Encoding.UTF8.GetByteCount(standardOutput));
            Assert.Equal(new string('x', maximumBytes), standardOutput);
        }
        finally
        {
            Directory.Delete(stagingDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task StandardOutputMaxPlusOneFailsClosedAndKillsJobDescendant()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        const int maximumBytes = 64;
        string canaryExecutable = ResolveCanaryExecutable();
        string appDirectory = Path.GetDirectoryName(canaryExecutable) ??
            throw new InvalidOperationException("Canary executable requires an application directory.");
        string stagingDirectory = Path.Combine(Path.GetTempPath(), $"ConvertyStdoutOverflow-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingDirectory);
        string childPidPath = Path.Combine(stagingDirectory, "child.pid");

        try
        {
            WorkerProcessLaunchRequest request = CreateRequest(
                canaryExecutable,
                appDirectory,
                [
                    "--stdout-bytes-spawn-child-and-hold",
                    (maximumBytes + 1).ToString(CultureInfo.InvariantCulture),
                    childPidPath,
                ],
                new WorkerFileSystemScope(stagingDirectory),
                maximumBytes);

            Exception failure = await Assert.ThrowsAnyAsync<Exception>(async () =>
                await new WindowsWorkerProcessLauncher().ExecuteAsync(
                    request,
                    TestContext.Current.CancellationToken));

            Assert.Equal("WorkerStandardOutputLimitExceededException", failure.GetType().Name);
            Assert.True(File.Exists(childPidPath), "Canary child PID was not recorded before stdout overflow.");
            int childPid = int.Parse(File.ReadAllText(childPidPath), NumberStyles.None, CultureInfo.InvariantCulture);
            await AssertProcessExitedAsync(childPid);
        }
        finally
        {
            Directory.Delete(stagingDirectory, recursive: true);
        }
    }

    private static WorkerProcessLaunchRequest CreateRequest(
        string executablePath,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        WorkerFileSystemScope scope,
        int maximumStandardOutputBytes)
    {
        ConstructorInfo constructor = Assert.Single(
            typeof(WorkerProcessLaunchRequest).GetConstructors(),
            candidate => candidate.GetParameters().Length == 9
                && candidate.GetParameters()[8].Name == "MaximumCapturedStandardOutputBytes");

        return (WorkerProcessLaunchRequest)constructor.Invoke(new object?[]
        {
            executablePath,
            workingDirectory,
            arguments,
            WorkerIsolationLevel.Strict,
            WorkerResourceLimits.ConversionDefault,
            scope,
            TimeSpan.FromSeconds(15),
            4096,
            maximumStandardOutputBytes,
        });
    }

    private static string ReadStandardOutput(WorkerProcessResult result)
    {
        PropertyInfo? property = result.GetType().GetProperty("StandardOutput");
        Assert.NotNull(property);
        return Assert.IsType<string>(property!.GetValue(result));
    }

    private static async Task AssertProcessExitedAsync(int processId)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using Process process = Process.GetProcessById(processId);
                if (process.HasExited)
                {
                    return;
                }
            }
            catch (ArgumentException)
            {
                return;
            }

            await Task.Delay(50, TestContext.Current.CancellationToken);
        }

        Assert.Fail($"Job descendant process {processId} survived stdout overflow.");
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

        Assert.True(File.Exists(path), $"Worker stdout canary executable is missing: {path}");
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
