using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Host.Ipc;
using Converty.Host.Jobs;
using Converty.Ipc.Protocol;
using Converty.Security.Ipc;
using Converty.Serialization;

namespace Converty.Host.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class HostPipeServerTests
{
    [Fact]
    public async Task SameUserConnectionHandlesOneBoundedRequest()
    {
        SecurityIdentifier userSid = CurrentUserSid();
        string pipeName = TestPipeName();
        var queue = new HostJobQueue(capacity: 4);
        var handler = new HostRequestHandler(queue);
        var validator = new ConnectedPeerValidator(new WindowsConnectedPeerIdentityReader());
        var server = new HostPipeServer(pipeName, userSid, validator, handler);
        ConversionRequest request = CreateRequest(Guid.NewGuid());

        Task<HostPipeSessionResult> serverTask = server.RunSingleConnectionAsync(TestContext.Current.CancellationToken);
        await using var client = CreateClient(pipeName);
        await client.ConnectAsync(TestContext.Current.CancellationToken);
        await ProtocolFrameCodec.WriteAsync(
            client,
            Encoding.UTF8.GetBytes(ContractJson.Serialize(request)),
            TestContext.Current.CancellationToken);
        ProtocolFrame response = await ProtocolFrameCodec.ReadAsync(client, TestContext.Current.CancellationToken);
        HostPipeSessionResult session = await serverTask;

        using JsonDocument document = JsonDocument.Parse(response.Payload);
        Assert.True(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal(HostPipeSessionResult.RequestHandled, session);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public async Task RejectedPeerNeverEnqueuesOrReadsRequestBody()
    {
        SecurityIdentifier userSid = CurrentUserSid();
        string pipeName = TestPipeName();
        var queue = new HostJobQueue(capacity: 4);
        var server = new HostPipeServer(
            pipeName,
            userSid,
            new RejectingPeerValidator(),
            new HostRequestHandler(queue));

        Task<HostPipeSessionResult> serverTask = server.RunSingleConnectionAsync(TestContext.Current.CancellationToken);
        await using var client = CreateClient(pipeName);
        await client.ConnectAsync(TestContext.Current.CancellationToken);
        HostPipeSessionResult result = await serverTask;

        Assert.Equal(HostPipeSessionResult.UnauthorizedPeer, result);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task MalformedFrameNeverEnqueues()
    {
        SecurityIdentifier userSid = CurrentUserSid();
        string pipeName = TestPipeName();
        var queue = new HostJobQueue(capacity: 4);
        var server = new HostPipeServer(
            pipeName,
            userSid,
            new AcceptingPeerValidator(),
            new HostRequestHandler(queue));

        Task<HostPipeSessionResult> serverTask = server.RunSingleConnectionAsync(TestContext.Current.CancellationToken);
        await using var client = CreateClient(pipeName);
        await client.ConnectAsync(TestContext.Current.CancellationToken);
        await client.WriteAsync(new byte[ProtocolLimits.HeaderSize], TestContext.Current.CancellationToken);
        await client.FlushAsync(TestContext.Current.CancellationToken);
        HostPipeSessionResult result = await serverTask;

        Assert.Equal(HostPipeSessionResult.InvalidFrame, result);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public async Task WaitingForClientHonorsCancellation()
    {
        SecurityIdentifier userSid = CurrentUserSid();
        var queue = new HostJobQueue(capacity: 1);
        var server = new HostPipeServer(
            TestPipeName(),
            userSid,
            new AcceptingPeerValidator(),
            new HostRequestHandler(queue));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await server.RunSingleConnectionAsync(cancellation.Token));
        Assert.Equal(0, queue.Count);
    }

    private static NamedPipeClientStream CreateClient(string pipeName) =>
        new(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous,
            TokenImpersonationLevel.Impersonation);

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

    private static string TestPipeName() => "converty.host.test." + Guid.NewGuid().ToString("N");

    private sealed class AcceptingPeerValidator : IConnectedPeerValidator
    {
        public bool IsExpectedUser(NamedPipeServerStream pipe, SecurityIdentifier expectedUser) => true;
    }

    private sealed class RejectingPeerValidator : IConnectedPeerValidator
    {
        public bool IsExpectedUser(NamedPipeServerStream pipe, SecurityIdentifier expectedUser) => false;
    }
}
