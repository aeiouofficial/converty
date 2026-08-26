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
    public async Task SubmitAsyncExchangesOneVersionedRequestAndResponseFrame()
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

        Task serverTask = ServeAcceptedResponseAsync(
            server,
            request.RequestId,
            expectedJobId,
            TestContext.Current.CancellationToken);
        var client = new BridgeClient(pipeName, TimeSpan.FromSeconds(5));

        BridgeSubmissionResult result = await client.SubmitAsync(request, TestContext.Current.CancellationToken);
        await serverTask;

        Assert.True(result.Accepted);
        Assert.Equal(expectedJobId, result.JobId);
        Assert.Null(result.Reason);
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
        var client = new BridgeClient(pipeName, TimeSpan.FromSeconds(5));

        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await client.SubmitAsync(request, TestContext.Current.CancellationToken));
        await serverTask;
    }

    [Fact]
    public async Task SubmitAsyncReportsConnectTimeoutAsHostUnavailable()
    {
        var client = new BridgeClient(TestPipeName(), TimeSpan.FromMilliseconds(100));
        ConversionRequest request = CreateRequest(Guid.NewGuid());

        BridgeHostUnavailableException error = await Assert.ThrowsAsync<BridgeHostUnavailableException>(async () =>
            await client.SubmitAsync(request, TestContext.Current.CancellationToken));

        Assert.IsType<TimeoutException>(error.InnerException);
    }

    [Fact]
    public void ForCurrentUserUsesSidQualifiedEndpoint()
    {
        SecurityIdentifier currentUser = CurrentUserSid();

        BridgeClient client = BridgeClient.ForCurrentUser(TimeSpan.FromSeconds(5));

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
}
