using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;
using Converty.Host.Ipc;
using Converty.Host.Jobs;
using Converty.Ipc.Protocol;
using Converty.Security.Ipc;
using Converty.Serialization;

namespace Converty.Host.Tests.Ipc;

[SupportedOSPlatform("windows")]
public sealed class HostSessionAcceptanceTests
{
    [Fact]
    public async Task AdmissionReplayAfterAmbiguousDisconnectReturnsOriginalJobOnFreshConnection()
    {
        SecurityIdentifier userSid = CurrentUserSid();
        string pipeName = TestPipeName();
        var queue = new HostJobQueue(capacity: 4);
        var validator = new AcceptingPeerValidator();
        ConversionRequest request = CreateRequest(Guid.NewGuid());
        byte[] payload = Encoding.UTF8.GetBytes(ContractJson.Serialize(request));

        HostPipeSessionResult firstSession = await SendAndDisconnectBeforeResponseAsync(
            pipeName,
            userSid,
            validator,
            queue,
            payload);

        Assert.Contains(firstSession, new[]
        {
            HostPipeSessionResult.RequestHandled,
            HostPipeSessionResult.TransportClosed,
        });
        Assert.Equal(1, queue.Count);

        ProtocolFrame replayFrame = await ExchangeOnFreshConnectionAsync(
            pipeName,
            userSid,
            validator,
            queue,
            payload);
        AdmissionResponse replay = ParseAdmissionResponse(replayFrame.Payload);

        Assert.True(replay.Accepted);
        Assert.NotEqual(Guid.Empty, replay.JobId);
        Assert.Null(replay.Reason);
        Assert.Equal(1, queue.Count);

        ProtocolFrame statusFrame = await ExchangeOnFreshConnectionAsync(
            pipeName,
            userSid,
            validator,
            queue,
            Encoding.UTF8.GetBytes(ContractJson.Serialize(new JobControlRequest(
                SchemaVersions.Current,
                JobControlOperation.Status,
                replay.JobId))));
        JobControlResponse status = ContractJson.DeserializeJobControlResponse(
            Encoding.UTF8.GetString(statusFrame.Payload.Span));

        Assert.True(status.Succeeded);
        Assert.Equal(replay.JobId, status.JobId);
        Assert.Equal(request.RequestId, status.Status?.RequestId);
        Assert.Equal(ConversionJobState.Queued, status.Status?.State);
    }

    [Fact]
    public async Task FreshConnectionsCanStatusCancelAndStatusAgainWithoutPersistentSession()
    {
        SecurityIdentifier userSid = CurrentUserSid();
        string pipeName = TestPipeName();
        var queue = new HostJobQueue(capacity: 4);
        var validator = new AcceptingPeerValidator();
        ConversionRequest request = CreateRequest(Guid.NewGuid());

        AdmissionResponse admission = ParseAdmissionResponse((await ExchangeOnFreshConnectionAsync(
            pipeName,
            userSid,
            validator,
            queue,
            Encoding.UTF8.GetBytes(ContractJson.Serialize(request)))).Payload);
        Assert.True(admission.Accepted);

        JobControlResponse queued = ParseJobControlResponse((await ExchangeOnFreshConnectionAsync(
            pipeName,
            userSid,
            validator,
            queue,
            SerializeJobControl(JobControlOperation.Status, admission.JobId))).Payload);
        Assert.True(queued.Succeeded);
        Assert.Equal(ConversionJobState.Queued, queued.Status?.State);

        JobControlResponse cancelled = ParseJobControlResponse((await ExchangeOnFreshConnectionAsync(
            pipeName,
            userSid,
            validator,
            queue,
            SerializeJobControl(JobControlOperation.Cancel, admission.JobId))).Payload);
        Assert.True(cancelled.Succeeded);
        Assert.Equal(ConversionJobState.Cancelled, cancelled.Status?.State);

        JobControlResponse afterCancel = ParseJobControlResponse((await ExchangeOnFreshConnectionAsync(
            pipeName,
            userSid,
            validator,
            queue,
            SerializeJobControl(JobControlOperation.Status, admission.JobId))).Payload);
        Assert.True(afterCancel.Succeeded);
        Assert.Equal(ConversionJobState.Cancelled, afterCancel.Status?.State);
        Assert.Equal(1, queue.Count);
    }

    private static async Task<HostPipeSessionResult> SendAndDisconnectBeforeResponseAsync(
        string pipeName,
        SecurityIdentifier userSid,
        IConnectedPeerValidator validator,
        HostJobQueue queue,
        byte[] payload)
    {
        var server = new HostPipeServer(pipeName, userSid, validator, new HostRequestHandler(queue));
        Task<HostPipeSessionResult> serverTask = server.RunSingleConnectionAsync(TestContext.Current.CancellationToken);

        await using (var client = CreateClient(pipeName))
        {
            await client.ConnectAsync(TestContext.Current.CancellationToken);
            await ProtocolFrameCodec.WriteAsync(client, payload, TestContext.Current.CancellationToken);
            await client.FlushAsync(TestContext.Current.CancellationToken);
        }

        return await serverTask;
    }

    private static async Task<ProtocolFrame> ExchangeOnFreshConnectionAsync(
        string pipeName,
        SecurityIdentifier userSid,
        IConnectedPeerValidator validator,
        HostJobQueue queue,
        byte[] payload)
    {
        var server = new HostPipeServer(pipeName, userSid, validator, new HostRequestHandler(queue));
        Task<HostPipeSessionResult> serverTask = server.RunSingleConnectionAsync(TestContext.Current.CancellationToken);

        await using var client = CreateClient(pipeName);
        await client.ConnectAsync(TestContext.Current.CancellationToken);
        await ProtocolFrameCodec.WriteAsync(client, payload, TestContext.Current.CancellationToken);
        ProtocolFrame response = await ProtocolFrameCodec.ReadAsync(client, TestContext.Current.CancellationToken);

        Assert.Equal(HostPipeSessionResult.RequestHandled, await serverTask);
        return response;
    }

    private static byte[] SerializeJobControl(JobControlOperation operation, Guid jobId) =>
        Encoding.UTF8.GetBytes(ContractJson.Serialize(new JobControlRequest(
            SchemaVersions.Current,
            operation,
            jobId)));

    private static JobControlResponse ParseJobControlResponse(ReadOnlyMemory<byte> payload) =>
        ContractJson.DeserializeJobControlResponse(Encoding.UTF8.GetString(payload.Span));

    private static AdmissionResponse ParseAdmissionResponse(ReadOnlyMemory<byte> payload)
    {
        using JsonDocument document = JsonDocument.Parse(payload);
        JsonElement root = document.RootElement;
        bool accepted = root.GetProperty("accepted").GetBoolean();
        Guid jobId = root.TryGetProperty("jobId", out JsonElement jobIdElement)
            ? Guid.Parse(jobIdElement.GetString()!)
            : Guid.Empty;
        string? reason = root.TryGetProperty("reason", out JsonElement reasonElement)
            ? reasonElement.GetString()
            : null;
        return new AdmissionResponse(accepted, jobId, reason);
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
            [@"C:\input\session-acceptance.wav"],
            targetFormat: null,
            presetId: null);

    private static SecurityIdentifier CurrentUserSid() =>
        WindowsIdentity.GetCurrent().User
        ?? throw new InvalidOperationException("Current Windows identity has no user SID.");

    private static string TestPipeName() => "converty.host.session.test." + Guid.NewGuid().ToString("N");

    private sealed record AdmissionResponse(bool Accepted, Guid JobId, string? Reason);

    private sealed class AcceptingPeerValidator : IConnectedPeerValidator
    {
        public bool IsExpectedUser(NamedPipeServerStream pipe, SecurityIdentifier expectedUser) => true;
    }
}
