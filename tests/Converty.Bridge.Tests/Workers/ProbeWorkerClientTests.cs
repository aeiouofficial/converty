using System.Text.Json;
using Converty.Bridge.Workers;
using Converty.Contracts.Conversion;
using Converty.Security.Workers;
using Converty.Serialization;

namespace Converty.Bridge.Tests.Workers;

public sealed class ProbeWorkerClientTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "converty-probe-worker-client-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ProbeUsesFixedStrictWorkerWithExactReadOnlyInputAndBoundedOutput()
    {
        Directory.CreateDirectory(_root);
        string workerPath = Path.Combine(_root, "Converty.ProbeWorker.exe");
        string stagedInput = Path.Combine(_root, "video ü ; $ (test).mp4");
        File.WriteAllBytes(workerPath, [0x4d, 0x5a]);
        File.WriteAllBytes(stagedInput, [0x00, 0x01, 0x02]);

        string strictResult = ContractJson.Serialize(
            MediaProbeResultV1.Failure(MediaProbeFailureReason.UnsupportedInput));
        var launcher = new RecordingLauncher(new WorkerProcessResult(0, string.Empty, strictResult));
        var client = new ProbeWorkerClient(workerPath, launcher);

        MediaProbeResultV1 result = await client.ProbeAsync(
            stagedInput,
            TimeSpan.FromSeconds(30),
            TestContext.Current.CancellationToken);

        Assert.Equal(MediaProbeStatus.Failure, result.Status);
        Assert.Equal(MediaProbeFailureReason.UnsupportedInput, result.FailureReason);
        WorkerProcessLaunchRequest request = Assert.IsType<WorkerProcessLaunchRequest>(launcher.Request);
        Assert.Equal(workerPath, request.ExecutablePath);
        Assert.Equal(_root, request.WorkingDirectory);
        Assert.Equal(WorkerIsolationLevel.Strict, request.IsolationLevel);
        Assert.Equal(stagedInput, request.FileSystemScope.ReadOnlyFile);
        Assert.Null(request.FileSystemScope.WritableDirectory);
        Assert.Equal(ProbeWorkerClient.MaximumCapturedErrorCharacters, request.MaximumCapturedStandardErrorCharacters);
        Assert.Equal(ProbeWorkerClient.MaximumCapturedStandardOutputBytes, request.MaximumCapturedStandardOutputBytes);
        Assert.True(request.MaximumCapturedStandardOutputBytes > 0);
        Assert.Equal(["--input", stagedInput], request.Arguments);
    }

    [Fact]
    public void ConstructorRejectsAnyWorkerFilenameOtherThanFixedProbeWorker()
    {
        Directory.CreateDirectory(_root);
        string wrongWorker = Path.Combine(_root, "probe-from-path.exe");
        File.WriteAllBytes(wrongWorker, [0x4d, 0x5a]);

        Assert.Throws<ArgumentException>(() =>
            new ProbeWorkerClient(wrongWorker, new RecordingLauncher(new WorkerProcessResult(0, string.Empty))));
    }

    [Fact]
    public async Task SuccessfulWorkerExitWithMalformedContractOutputFailsClosed()
    {
        Directory.CreateDirectory(_root);
        string workerPath = Path.Combine(_root, "Converty.ProbeWorker.exe");
        string stagedInput = Path.Combine(_root, "input.mp4");
        File.WriteAllBytes(workerPath, [0x4d, 0x5a]);
        File.WriteAllBytes(stagedInput, [0x00]);
        var client = new ProbeWorkerClient(
            workerPath,
            new RecordingLauncher(new WorkerProcessResult(0, string.Empty, "{\"schemaVersion\":1}")));

        await Assert.ThrowsAsync<JsonException>(() =>
            client.ProbeAsync(stagedInput, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task NonzeroWorkerExitBecomesBoundedProbeFailureWithoutParsingStdout()
    {
        Directory.CreateDirectory(_root);
        string workerPath = Path.Combine(_root, "Converty.ProbeWorker.exe");
        string stagedInput = Path.Combine(_root, "input.mp4");
        File.WriteAllBytes(workerPath, [0x4d, 0x5a]);
        File.WriteAllBytes(stagedInput, [0x00]);
        var client = new ProbeWorkerClient(
            workerPath,
            new RecordingLauncher(new WorkerProcessResult(4, "backend details must not cross", "not-json")));

        MediaProbeResultV1 result = await client.ProbeAsync(
            stagedInput,
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        Assert.Equal(MediaProbeStatus.Failure, result.Status);
        Assert.Equal(MediaProbeFailureReason.ProbeFailed, result.FailureReason);
        Assert.Null(result.Facts);
    }

    [Fact]
    public async Task LauncherTimeoutPropagatesFailClosed()
    {
        Directory.CreateDirectory(_root);
        string workerPath = Path.Combine(_root, "Converty.ProbeWorker.exe");
        string stagedInput = Path.Combine(_root, "input.mp4");
        File.WriteAllBytes(workerPath, [0x4d, 0x5a]);
        File.WriteAllBytes(stagedInput, [0x00]);
        var client = new ProbeWorkerClient(workerPath, new ThrowingLauncher(new TimeoutException("bounded timeout")));

        await Assert.ThrowsAsync<TimeoutException>(() =>
            client.ProbeAsync(stagedInput, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken));
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

    private sealed class ThrowingLauncher(Exception exception) : IWorkerProcessLauncher
    {
        public Task<WorkerProcessResult> ExecuteAsync(
            WorkerProcessLaunchRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromException<WorkerProcessResult>(exception);
    }
}
