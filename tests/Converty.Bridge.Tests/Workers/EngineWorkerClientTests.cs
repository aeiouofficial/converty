using System.Reflection;
using Converty.Bridge.Workers;
using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Execution;
using Converty.Security.Workers;

namespace Converty.Bridge.Tests.Workers;

public sealed class EngineWorkerClientTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "converty-engine-worker-client-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ModeAwareExecutionUsesExactBoundedWorkerSurface()
    {
        Directory.CreateDirectory(_root);
        string workerPath = Path.Combine(_root, "Converty.EngineWorker.exe");
        string stagedInput = Path.Combine(_root, "video ü ; $ (test).mov");
        string stagedOutput = Path.Combine(_root, "video ü ; $ (test).partial.mp4");
        File.WriteAllBytes(workerPath, [0x4d, 0x5a]);
        File.WriteAllBytes(stagedInput, [0x00, 0x01, 0x02]);

        var launcher = new RecordingLauncher(new WorkerProcessResult(0, string.Empty));
        var client = new EngineWorkerClient(workerPath, launcher);
        MethodInfo? modeAwareExecute = typeof(EngineWorkerClient).GetMethod(
            "ExecuteAsync",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types:
            [
                typeof(PresetId),
                typeof(ConversionMode),
                typeof(string),
                typeof(string),
                typeof(TimeSpan),
                typeof(CancellationToken),
            ],
            modifiers: null);

        Assert.NotNull(modeAwareExecute);
        object? invocation = modeAwareExecute!.Invoke(
            client,
            [
                PresetId.Parse("video.mp4.h264"),
                ConversionMode.Remux,
                stagedInput,
                stagedOutput,
                TimeSpan.FromSeconds(30),
                TestContext.Current.CancellationToken,
            ]);
        Task<ConversionWorkerResult> task = Assert.IsAssignableFrom<Task<ConversionWorkerResult>>(invocation);
        ConversionWorkerResult result = await task;

        Assert.True(result.Succeeded);
        WorkerProcessLaunchRequest request = Assert.IsType<WorkerProcessLaunchRequest>(launcher.Request);
        Assert.Equal(workerPath, request.ExecutablePath);
        Assert.Equal(_root, request.WorkingDirectory);
        Assert.Equal(WorkerIsolationLevel.Strict, request.IsolationLevel);
        Assert.Equal(_root, request.FileSystemScope.WritableDirectory);
        Assert.Null(request.FileSystemScope.ReadOnlyFile);
        Assert.Equal(
            [
                "--preset",
                "video.mp4.h264",
                "--mode",
                "remux",
                "--input",
                stagedInput,
                "--output",
                stagedOutput,
            ],
            request.Arguments);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class RecordingLauncher(WorkerProcessResult result) : IWorkerProcessLauncher
    {
        public WorkerProcessLaunchRequest? Request { get; private set; }

        public Task<WorkerProcessResult> ExecuteAsync(
            WorkerProcessLaunchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            return Task.FromResult(result);
        }
    }
}
