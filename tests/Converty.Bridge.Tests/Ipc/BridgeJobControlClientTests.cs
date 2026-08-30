using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Text;
using Converty.Bridge.Ipc;
using Converty.Contracts;
using Converty.Contracts.Jobs;
using Converty.Ipc.Protocol;
using Converty.Security.Ipc;
using Converty.Serialization;

namespace Converty.Bridge.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class BridgeJobControlClientTests
{
    [Fact]
    public async Task GetStatusAsyncSendsStatusAndAcceptsCorrelatedResponse()
    {
        string pipeName = TestPipeName();
        Guid jobId = Guid.NewGuid();
        JobStatusSnapshot status = Status(jobId, ConversionJobState.Converting);
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Status,
            jobId,
            succeeded: true,
            status,
            reason: null);
        using var server = CreateServer(pipeName);
        var verifier = new RecordingVerifier();
        Task serverTask = ServeControlResponseAsync(
            server,
            JobControlOperation.Status,
            jobId,
            response,
            TestContext.Current.CancellationToken);
        BridgeClient client = new(pipeName, TimeSpan.FromSeconds(5), verifier);

        JobControlResponse result = await client.GetStatusAsync(jobId, TestContext.Current.CancellationToken);
        await serverTask;

        Assert.True(result.Succeeded);
        Assert.Equal(JobControlOperation.Status, result.Operation);
        Assert.Equal(jobId, result.JobId);
        Assert.Equal(ConversionJobState.Converting, result.Status!.State);
        Assert.Equal(1, verifier.CallCount);
        Assert.True(verifier.SawConnectedPipe);
    }

    [Fact]
    public async Task CancelAsyncSendsCancelAndAcceptsCancelledResponse()
    {
        string pipeName = TestPipeName();
        Guid jobId = Guid.NewGuid();
        JobStatusSnapshot status = Status(jobId, ConversionJobState.Cancelled);
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Cancel,
            jobId,
            succeeded: true,
            status,
            reason: null);
        using var server = CreateServer(pipeName);
        var verifier = new RecordingVerifier();
        Task serverTask = ServeControlResponseAsync(
            server,
            JobControlOperation.Cancel,
            jobId,
            response,
            TestContext.Current.CancellationToken);
        BridgeClient client = new(pipeName, TimeSpan.FromSeconds(5), verifier);

        JobControlResponse result = await client.CancelAsync(jobId, TestContext.Current.CancellationToken);
        await serverTask;

        Assert.True(result.Succeeded);
        Assert.Equal(JobControlOperation.Cancel, result.Operation);
        Assert.Equal(ConversionJobState.Cancelled, result.Status!.State);
        Assert.Equal(1, verifier.CallCount);
        Assert.True(verifier.SawConnectedPipe);
    }

    [Theory]
    [InlineData(JobControlOperation.Status)]
    [InlineData(JobControlOperation.Cancel)]
    public async Task IdentityRejectionWritesNoControlApplicationFrame(JobControlOperation operation)
    {
        string pipeName = TestPipeName();
        Guid jobId = Guid.NewGuid();
        using var server = CreateServer(pipeName);
        var verifier = new RecordingVerifier(new BridgeServerIdentityException("fake server"));
        BridgeClient client = new(pipeName, TimeSpan.FromSeconds(5), verifier);
        Task serverTask = AssertConnectionClosesWithoutPayloadAsync(server, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<BridgeServerIdentityException>(() => operation == JobControlOperation.Status
            ? client.GetStatusAsync(jobId, TestContext.Current.CancellationToken)
            : client.CancelAsync(jobId, TestContext.Current.CancellationToken));
        await serverTask;

        Assert.Equal(1, verifier.CallCount);
    }

    [Fact]
    public async Task ResponseOperationMismatchIsRejected()
    {
        Guid requestedJobId = Guid.NewGuid();
        JobStatusSnapshot status = Status(requestedJobId, ConversionJobState.Cancelled);
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Cancel,
            requestedJobId,
            succeeded: true,
            status,
            reason: null);

        await AssertInvalidStatusResponseAsync(requestedJobId, response);
    }

    [Fact]
    public async Task ResponseJobIdMismatchIsRejected()
    {
        Guid requestedJobId = Guid.NewGuid();
        Guid responseJobId = Guid.NewGuid();
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Status,
            responseJobId,
            succeeded: true,
            Status(responseJobId, ConversionJobState.Queued),
            reason: null);

        await AssertInvalidStatusResponseAsync(requestedJobId, response);
    }

    [Theory]
    [InlineData(JobControlFailureReason.NotCancellable, ConversionJobState.Converting)]
    [InlineData(JobControlFailureReason.PersistenceFailure, ConversionJobState.Queued)]
    public async Task StatusRejectsCancelOnlyFailureReasons(
        JobControlFailureReason reason,
        ConversionJobState state)
    {
        Guid jobId = Guid.NewGuid();
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Status,
            jobId,
            succeeded: false,
            Status(jobId, state),
            reason);

        await AssertInvalidStatusResponseAsync(jobId, response);
    }

    [Fact]
    public async Task CancelSuccessRequiresCancelledStatus()
    {
        Guid jobId = Guid.NewGuid();
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Cancel,
            jobId,
            succeeded: true,
            Status(jobId, ConversionJobState.Converting),
            reason: null);

        await AssertInvalidCancelResponseAsync(jobId, response);
    }

    [Fact]
    public async Task CancelPersistenceFailureRequiresQueuedStatus()
    {
        Guid jobId = Guid.NewGuid();
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Cancel,
            jobId,
            succeeded: false,
            Status(jobId, ConversionJobState.Converting),
            JobControlFailureReason.PersistenceFailure);

        await AssertInvalidCancelResponseAsync(jobId, response);
    }

    [Fact]
    public async Task CancelNotCancellableRequiresNonQueuedStatus()
    {
        Guid jobId = Guid.NewGuid();
        var response = new JobControlResponse(
            SchemaVersions.Current,
            JobControlOperation.Cancel,
            jobId,
            succeeded: false,
            Status(jobId, ConversionJobState.Queued),
            JobControlFailureReason.NotCancellable);

        await AssertInvalidCancelResponseAsync(jobId, response);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"succeeded\":false,\"reason\":\"jobNotFound\",\"extra\":true}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"operation\":\"cancel\",\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"succeeded\":false,\"reason\":\"jobNotFound\"}")]
    [InlineData("{\"schemaVersion\":1,\"operation\":\"status\",\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"succeeded\":true,\"status\":{\"schemaVersion\":1,\"jobId\":\"11111111-1111-1111-1111-111111111111\",\"requestId\":\"22222222-2222-2222-2222-222222222222\",\"state\":\"unknown\"}}")]
    public async Task MalformedOrStrictlyInvalidControlResponseIsRejected(string responseJson)
    {
        string pipeName = TestPipeName();
        Guid jobId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        using var server = CreateServer(pipeName);
        Task serverTask = ServeRawControlResponseAsync(
            server,
            JobControlOperation.Status,
            jobId,
            Encoding.UTF8.GetBytes(responseJson),
            TestContext.Current.CancellationToken);
        BridgeClient client = new(
            pipeName,
            TimeSpan.FromSeconds(5),
            new RecordingVerifier());

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            client.GetStatusAsync(jobId, TestContext.Current.CancellationToken));
        await serverTask;
    }

    private static async Task AssertInvalidStatusResponseAsync(Guid jobId, JobControlResponse response)
    {
        string pipeName = TestPipeName();
        using var server = CreateServer(pipeName);
        Task serverTask = ServeControlResponseAsync(
            server,
            JobControlOperation.Status,
            jobId,
            response,
            TestContext.Current.CancellationToken);
        BridgeClient client = new(
            pipeName,
            TimeSpan.FromSeconds(5),
            new RecordingVerifier());

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            client.GetStatusAsync(jobId, TestContext.Current.CancellationToken));
        await serverTask;
    }

    private static async Task AssertInvalidCancelResponseAsync(Guid jobId, JobControlResponse response)
    {
        string pipeName = TestPipeName();
        using var server = CreateServer(pipeName);
        Task serverTask = ServeControlResponseAsync(
            server,
            JobControlOperation.Cancel,
            jobId,
            response,
            TestContext.Current.CancellationToken);
        BridgeClient client = new(
            pipeName,
            TimeSpan.FromSeconds(5),
            new RecordingVerifier());

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            client.CancelAsync(jobId, TestContext.Current.CancellationToken));
        await serverTask;
    }

    private static async Task ServeControlResponseAsync(
        NamedPipeServerStream server,
        JobControlOperation expectedOperation,
        Guid expectedJobId,
        JobControlResponse response,
        CancellationToken cancellationToken) =>
        await ServeRawControlResponseAsync(
            server,
            expectedOperation,
            expectedJobId,
            Encoding.UTF8.GetBytes(ContractJson.Serialize(response)),
            cancellationToken);

    private static async Task ServeRawControlResponseAsync(
        NamedPipeServerStream server,
        JobControlOperation expectedOperation,
        Guid expectedJobId,
        byte[] response,
        CancellationToken cancellationToken)
    {
        await server.WaitForConnectionAsync(cancellationToken);
        ProtocolFrame frame = await ProtocolFrameCodec.ReadAsync(server, cancellationToken);
        JobControlRequest request = ContractJson.DeserializeJobControlRequest(
            Encoding.UTF8.GetString(frame.Payload.Span));
        Assert.Equal(expectedOperation, request.Operation);
        Assert.Equal(expectedJobId, request.JobId);
        await ProtocolFrameCodec.WriteAsync(server, response, cancellationToken);
    }

    private static async Task AssertConnectionClosesWithoutPayloadAsync(
        NamedPipeServerStream server,
        CancellationToken cancellationToken)
    {
        await server.WaitForConnectionAsync(cancellationToken);
        byte[] buffer = new byte[1];
        int read = await server.ReadAsync(buffer, cancellationToken);
        Assert.Equal(0, read);
    }

    private static NamedPipeServerStream CreateServer(string pipeName) =>
        new(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

    private static JobStatusSnapshot Status(Guid jobId, ConversionJobState state) =>
        new(
            SchemaVersions.Current,
            jobId,
            Guid.NewGuid(),
            state,
            progress: null,
            message: null);

    private static string TestPipeName() =>
        "converty.bridge.control.test." + Guid.NewGuid().ToString("N");

    private sealed class RecordingVerifier(Exception? error = null) : IConnectedServerIdentityVerifier
    {
        public int CallCount { get; private set; }
        public bool SawConnectedPipe { get; private set; }

        public void VerifyConnectedServer(NamedPipeClientStream pipe)
        {
            CallCount++;
            SawConnectedPipe |= pipe.IsConnected;
            if (error is not null)
            {
                throw error;
            }
        }
    }
}
