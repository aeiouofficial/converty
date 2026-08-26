using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Converty.Bridge.Ipc;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Ipc.Protocol;
using Converty.Security.Ipc;
using Converty.Serialization;

namespace Converty.Bridge.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class BridgeClientTests
{
    [Fact]
    public async Task SubmitAsyncVerifiesConnectedServerBeforeFirstApplicationFrame()
    {
        string pipeName = TestPipeName();
        Guid expectedJobId = Guid.NewGuid();
        ConversionRequest request = CreateRequest(Guid.NewGuid());
        using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        var verifier = new RecordingVerifier();

        Task serverTask = ServeAcceptedResponseAsync(
            server,
            request.RequestId,
            expectedJobId,
            TestContext.Current.CancellationToken);
        var client = new BridgeClient(pipeName, TimeSpan.FromSeconds(5), verifier);

        BridgeSubmissionResult result = await client.SubmitAsync(request, TestContext.Current.CancellationToken);
        await serverTask;

        Assert.True(result.Accepted);
        Assert.Equal(expectedJobId, result.JobId);
        Assert.Null(result.Reason);
        Assert.Equal(1, verifier.CallCount);
        Assert.True(verifier.SawConnectedPipe);
    }

    [Fact]
    public async Task IdentityRejectionWritesNoApplicationFrame()
    {
        string pipeName = TestPipeName();
        ConversionRequest request = CreateRequest(Guid.NewGuid());
        using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        var verifier = new RecordingVerifier(new BridgeServerIdentityException("fake server"));
        var client = new BridgeClient(pipeName, TimeSpan.FromSeconds(5), verifier);

        Task serverTask = AssertConnectionClosesWithoutPayloadAsync(server, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<BridgeServerIdentityException>(
            () => client.SubmitAsync(request, TestContext.Current.CancellationToken));
        await serverTask;

        Assert.Equal(1, verifier.CallCount);
    }

    [Fact]
    public async Task SubmitAsyncFailsWhenResponseSchemaVersionIsUnsupported()
    {
        string pipeName = TestPipeName();
        ConversionRequest request = CreateRequest(Guid.NewGuid());
        using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        Task serverTask = ServeResponseAsync(
            server,
            JsonSerializer.SerializeToUtf8Bytes(new
            {
                schemaVersion = SchemaVersions.Current + 1,
                accepted = true,
                jobId = Guid.NewGuid().ToString("D"),
            }),
            TestContext.Current.CancellationToken);
        var client = new BridgeClient(pipeName, TimeSpan.FromSeconds(5), new RecordingVerifier());

        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await client.SubmitAsync(request, TestContext.Current.CancellationToken));
        await serverTask;
    }

    [Fact]
    public async Task SubmitAsyncReportsConnectTimeoutAsHostUnavailableWithoutRunningIdentityVerifier()
    {
        var verifier = new RecordingVerifier();
        var client = new BridgeClient(TestPipeName(), TimeSpan.FromMilliseconds(100), verifier);
        ConversionRequest request = CreateRequest(Guid.NewGuid());

        BridgeHostUnavailableException error = await Assert.ThrowsAsync<BridgeHostUnavailableException>(async () =>
            await client.SubmitAsync(request, TestContext.Current.CancellationToken));

        Assert.IsType<TimeoutException>(error.InnerException);
        Assert.Equal(0, verifier.CallCount);
    }

    [Fact]
    public void ForCurrentUserUsesSidQualifiedEndpoint()
    {
        SecurityIdentifier currentUser = CurrentUserSid();
        var verifier = new RecordingVerifier();

        BridgeClient client = BridgeClient.ForCurrentUser(TimeSpan.FromSeconds(5), verifier);

        Assert.Equal(PipeEndpointName.ForUser(currentUser), client.PipeName);
    }

    private static async Task ServeAcceptedResponseAsync(
        NamedPipeServerStream server,
        Guid expectedRequestId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        await server.WaitForConnectionAsync(cancellationToken);
        ProtocolFrame frame = await ProtocolFrameCodec.ReadAsync(server, cancellationToken);
        ConversionRequest received = ContractJson.DeserializeConversionRequest(Encoding.UTF8.GetString(frame.Payload.Span));
        Assert.Equal(expectedRequestId, received.RequestId);

        byte[] response = JsonSerializer.SerializeToUtf8Bytes(new
        {
            schemaVersion = SchemaVersions.Current,
            accepted = true,
            jobId = jobId.ToString("D"),
        });
        await ProtocolFrameCodec.WriteAsync(server, response, cancellationToken);
    }

    private static async Task ServeResponseAsync(
        NamedPipeServerStream server,
        byte[] response,
        CancellationToken cancellationToken)
    {
        await server.WaitForConnectionAsync(cancellationToken);
        _ = await ProtocolFrameCodec.ReadAsync(server, cancellationToken);
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

    private static ConversionRequest CreateRequest(Guid requestId) =>
        new(
            SchemaVersions.Current,
            requestId,
            ConversionAction.ConvertUsingDefault,
            [@"C:\input\sample.wav"],
            targetFormat: null,
            presetId: null);

    private static SecurityIdentifier CurrentUserSid() =>
        WindowsIdentity.GetCurrent().User
        ?? throw new InvalidOperationException("Current Windows identity has no user SID.");

    private static string TestPipeName() => "converty.bridge.test." + Guid.NewGuid().ToString("N");

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
