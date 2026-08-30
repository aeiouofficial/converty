using System.Text;
using System.Text.Json;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;
using Converty.Host.Ipc;
using Converty.Host.Jobs;
using Converty.Serialization;

namespace Converty.Host.Tests.Ipc;

public sealed class HostJobControlHandlerTests
{
    [Fact]
    public async Task StatusFoundReturnsCurrentSnapshot()
    {
        var queue = new HostJobQueue(capacity: 2);
        ConversionRequest request = CreateRequest(Guid.NewGuid());
        JobAdmissionResult admission = queue.TryEnqueue(request);
        var handler = new HostRequestHandler(queue);

        byte[] response = await handler.HandleAsync(
            ControlPayload(JobControlOperation.Status, admission.JobId),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        JobControlResponse result = ParseControl(response);
        Assert.True(result.Succeeded);
        Assert.Equal(JobControlOperation.Status, result.Operation);
        Assert.Equal(admission.JobId, result.JobId);
        Assert.NotNull(result.Status);
        Assert.Equal(request.RequestId, result.Status.RequestId);
        Assert.Equal(ConversionJobState.Queued, result.Status.State);
        Assert.Null(result.Reason);
    }

    [Fact]
    public async Task StatusUnknownReturnsJobNotFoundWithoutStatus()
    {
        var queue = new HostJobQueue(capacity: 2);
        var handler = new HostRequestHandler(queue);
        Guid jobId = Guid.NewGuid();

        byte[] response = await handler.HandleAsync(
            ControlPayload(JobControlOperation.Status, jobId),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        JobControlResponse result = ParseControl(response);
        Assert.False(result.Succeeded);
        Assert.Equal(JobControlOperation.Status, result.Operation);
        Assert.Equal(jobId, result.JobId);
        Assert.Equal(JobControlFailureReason.JobNotFound, result.Reason);
        Assert.Null(result.Status);
    }

    [Fact]
    public async Task QueuedCancelReturnsCancelledAndStoresCancelledState()
    {
        var queue = new HostJobQueue(capacity: 2);
        JobAdmissionResult admission = queue.TryEnqueue(CreateRequest(Guid.NewGuid()));
        var handler = new HostRequestHandler(queue);

        byte[] response = await handler.HandleAsync(
            ControlPayload(JobControlOperation.Cancel, admission.JobId),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        JobControlResponse result = ParseControl(response);
        Assert.True(result.Succeeded);
        Assert.Equal(JobControlOperation.Cancel, result.Operation);
        Assert.Equal(admission.JobId, result.JobId);
        Assert.NotNull(result.Status);
        Assert.Equal(ConversionJobState.Cancelled, result.Status.State);
        Assert.Null(result.Reason);
        Assert.True(queue.TryGet(admission.JobId, out JobStatusSnapshot? stored));
        Assert.Equal(ConversionJobState.Cancelled, stored!.State);
    }

    [Fact]
    public async Task CancelUnknownReturnsJobNotFound()
    {
        var queue = new HostJobQueue(capacity: 2);
        var handler = new HostRequestHandler(queue);
        Guid jobId = Guid.NewGuid();

        byte[] response = await handler.HandleAsync(
            ControlPayload(JobControlOperation.Cancel, jobId),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        JobControlResponse result = ParseControl(response);
        Assert.False(result.Succeeded);
        Assert.Equal(JobControlFailureReason.JobNotFound, result.Reason);
        Assert.Null(result.Status);
    }

    [Fact]
    public async Task CancelNonQueuedReturnsNotCancellableWithCurrentStatus()
    {
        Guid jobId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();
        var recovered = new JobStatusSnapshot(
            SchemaVersions.Current,
            jobId,
            requestId,
            ConversionJobState.Converting,
            0.5,
            "Worker is running.");
        var queue = new HostJobQueue(capacity: 1, new TestJournal([recovered]));
        var handler = new HostRequestHandler(queue);

        byte[] response = await handler.HandleAsync(
            ControlPayload(JobControlOperation.Cancel, jobId),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        JobControlResponse result = ParseControl(response);
        Assert.False(result.Succeeded);
        Assert.Equal(JobControlFailureReason.NotCancellable, result.Reason);
        Assert.NotNull(result.Status);
        Assert.Equal(ConversionJobState.Converting, result.Status.State);
        Assert.Equal(jobId, result.Status.JobId);
    }

    [Fact]
    public async Task CancelPersistenceFailureLeavesQueuedAndReturnsPersistenceFailure()
    {
        var journal = new TestJournal([], failOnCommit: 2);
        var queue = new HostJobQueue(capacity: 1, journal);
        JobAdmissionResult admission = queue.TryEnqueue(CreateRequest(Guid.NewGuid()));
        Assert.True(admission.Accepted);
        var handler = new HostRequestHandler(queue);

        byte[] response = await handler.HandleAsync(
            ControlPayload(JobControlOperation.Cancel, admission.JobId),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        JobControlResponse result = ParseControl(response);
        Assert.False(result.Succeeded);
        Assert.Equal(JobControlFailureReason.PersistenceFailure, result.Reason);
        Assert.NotNull(result.Status);
        Assert.Equal(ConversionJobState.Queued, result.Status.State);
        Assert.True(queue.TryGet(admission.JobId, out JobStatusSnapshot? stored));
        Assert.Equal(ConversionJobState.Queued, stored!.State);
    }

    [Fact]
    public async Task HybridControlAndConversionJsonReturnsInvalidRequestWithoutMutation()
    {
        var queue = new HostJobQueue(capacity: 2);
        var handler = new HostRequestHandler(queue);
        Guid jobId = Guid.NewGuid();
        string json = $"{{\"schemaVersion\":1,\"operation\":\"status\",\"jobId\":\"{jobId:D}\",\"requestId\":\"{Guid.NewGuid():D}\",\"action\":\"convertUsingDefault\",\"files\":[\"C:\\\\input\\\\sample.wav\"]}}";

        byte[] response = await handler.HandleAsync(
            Encoding.UTF8.GetBytes(json),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(response);
        Assert.False(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal("invalidRequest", document.RootElement.GetProperty("reason").GetString());
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task UnsupportedControlJsonReturnsInvalidRequestWithoutMutation()
    {
        var queue = new HostJobQueue(capacity: 2);
        var handler = new HostRequestHandler(queue);
        string json = $"{{\"schemaVersion\":999,\"operation\":\"status\",\"jobId\":\"{Guid.NewGuid():D}\"}}";

        byte[] response = await handler.HandleAsync(
            Encoding.UTF8.GetBytes(json),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(response);
        Assert.False(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal("invalidRequest", document.RootElement.GetProperty("reason").GetString());
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task UnauthorizedControlDoesNotRevealJobExistence()
    {
        var queue = new HostJobQueue(capacity: 2);
        JobAdmissionResult admission = queue.TryEnqueue(CreateRequest(Guid.NewGuid()));
        var handler = new HostRequestHandler(queue);

        byte[] response = await handler.HandleAsync(
            ControlPayload(JobControlOperation.Status, admission.JobId),
            PeerAuthorization.Rejected,
            TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(response);
        Assert.False(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal("unauthorizedPeer", document.RootElement.GetProperty("reason").GetString());
        Assert.Equal(1, queue.Count);
    }

    private static byte[] ControlPayload(JobControlOperation operation, Guid jobId) =>
        Encoding.UTF8.GetBytes(ContractJson.Serialize(new JobControlRequest(
            SchemaVersions.Current,
            operation,
            jobId)));

    private static JobControlResponse ParseControl(byte[] response) =>
        ContractJson.DeserializeJobControlResponse(Encoding.UTF8.GetString(response));

    private static ConversionRequest CreateRequest(Guid requestId) =>
        new(
            SchemaVersions.Current,
            requestId,
            ConversionAction.ConvertUsingDefault,
            [@"C:\input\sample.wav"],
            targetFormat: null,
            presetId: null);

    private sealed class TestJournal(
        IReadOnlyList<JobStatusSnapshot> recovered,
        int failOnCommit = -1) : IHostJobJournal
    {
        private int _commitCount;

        public IReadOnlyList<JobStatusSnapshot> LoadForRecovery() => recovered;

        public void Commit(IReadOnlyCollection<JobStatusSnapshot> snapshots)
        {
            _commitCount++;
            if (_commitCount == failOnCommit)
            {
                throw new IOException("Injected journal failure.");
            }
        }
    }
}
