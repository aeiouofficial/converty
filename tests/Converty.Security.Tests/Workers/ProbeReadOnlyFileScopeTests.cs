using System.Reflection;
using Converty.Security.Workers;

namespace Converty.Security.Tests.Workers;

public sealed class ProbeReadOnlyFileScopeTests
{
    [Fact]
    public void ReadOnlyFileFactoryAcceptsExactExistingFileAndRejectsInvalidInputs()
    {
        MethodInfo factory = RequireFactory();
        string root = Path.Combine(Path.GetTempPath(), $"ConvertyProbeScope-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "input.bin");
        File.WriteAllText(input, "probe-input");

        try
        {
            WorkerFileSystemScope scope = InvokeFactory(factory, input);
            PropertyInfo? property = typeof(WorkerFileSystemScope).GetProperty("ReadOnlyFile");
            Assert.NotNull(property);
            Assert.Equal(Path.GetFullPath(input), property!.GetValue(scope));

            AssertInvocationThrows<ArgumentException>(() => InvokeFactory(factory, "input.bin"));
            AssertInvocationThrows<FileNotFoundException>(() => InvokeFactory(factory, Path.Combine(root, "missing.bin")));
            AssertInvocationThrows<ArgumentException>(() => InvokeFactory(factory, root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task StrictReadOnlyScopeAllowsExactInputButDeniesMutationSiblingProfileAndOutsideWrites()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        MethodInfo factory = RequireFactory();
        string canaryExecutable = ResolveCanaryExecutable();
        string appDirectory = Path.GetDirectoryName(canaryExecutable) ??
            throw new InvalidOperationException("Canary executable requires an application directory.");
        string root = Path.Combine(Path.GetTempPath(), $"ConvertyProbeScopeStrict-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "input.bin");
        string sibling = Path.Combine(root, "sibling.bin");
        File.WriteAllText(input, "probe-input");
        File.WriteAllText(sibling, "must-remain-inaccessible");

        string profileFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            $"ConvertyProbeDenied-{Guid.NewGuid():N}.txt");
        File.WriteAllText(profileFile, "profile-secret-canary");

        try
        {
            WorkerFileSystemScope scope = InvokeFactory(factory, input);
            var launcher = new WindowsWorkerProcessLauncher();

            Assert.Equal(0, (await launcher.ExecuteAsync(CreateRequest(canaryExecutable, appDirectory, ["--read-file", input], scope), TestContext.Current.CancellationToken)).ExitCode);
            Assert.Equal(13, (await launcher.ExecuteAsync(CreateRequest(canaryExecutable, appDirectory, ["--write-file", input], scope), TestContext.Current.CancellationToken)).ExitCode);
            Assert.Equal(13, (await launcher.ExecuteAsync(CreateRequest(canaryExecutable, appDirectory, ["--read-file", sibling], scope), TestContext.Current.CancellationToken)).ExitCode);
            Assert.Equal(13, (await launcher.ExecuteAsync(CreateRequest(canaryExecutable, appDirectory, ["--write-file", sibling], scope), TestContext.Current.CancellationToken)).ExitCode);
            Assert.Equal(13, (await launcher.ExecuteAsync(CreateRequest(canaryExecutable, appDirectory, ["--read-file", profileFile], scope), TestContext.Current.CancellationToken)).ExitCode);

            Assert.Equal("probe-input", File.ReadAllText(input));
            Assert.Equal("must-remain-inaccessible", File.ReadAllText(sibling));
            Assert.Equal("profile-secret-canary", File.ReadAllText(profileFile));
        }
        finally
        {
            File.Delete(profileFile);
            Directory.Delete(root, recursive: true);
        }
    }

    private static WorkerProcessLaunchRequest CreateRequest(
        string executablePath,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        WorkerFileSystemScope scope) => new(
            executablePath,
            workingDirectory,
            arguments,
            WorkerIsolationLevel.Strict,
            WorkerResourceLimits.ConversionDefault,
            scope,
            TimeSpan.FromSeconds(10),
            MaximumCapturedStandardErrorCharacters: 4096);

    private static MethodInfo RequireFactory() => Assert.Single(
        typeof(WorkerFileSystemScope).GetMethods(BindingFlags.Public | BindingFlags.Static),
        method => method.Name == "ForReadOnlyFile"
            && method.GetParameters().Length == 1
            && method.GetParameters()[0].ParameterType == typeof(string));

    private static WorkerFileSystemScope InvokeFactory(MethodInfo factory, string path)
    {
        try
        {
            return Assert.IsType<WorkerFileSystemScope>(factory.Invoke(null, new object?[] { path }));
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static TException AssertInvocationThrows<TException>(Action action)
        where TException : Exception => Assert.Throws<TException>(action);

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

        Assert.True(File.Exists(path), $"Worker probe-scope canary executable is missing: {path}");
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
