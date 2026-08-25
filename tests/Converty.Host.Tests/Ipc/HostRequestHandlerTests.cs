using System.Text;
using System.Text.Json;
using Converty.Contracts.Conversion;
using Converty.Host.Ipc;
using Converty.Host.Jobs;
using Converty.Serialization;

namespace Converty.Host.Tests.Ipc;

public sealed class HostRequestHandlerTests
{
    [Fact]
    public async Task AuthorizedValidRequestEnqueuesExactlyOnce()
    {
        var queue = new HostJobQueue(capacity: 4);
        var handler = new HostRequestHandler(queue);
        byte[] payload = ValidPayload(Guid.NewGuid());

        byte[] response = await handler.HandleAsync(payload, PeerAuthorization.ExpectedUser, TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(response);
        Assert.True(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public async Task UnauthorizedRequestDoesNotEnqueue()
    {
        var queue = new HostJobQueue(capacity: 4);
        var handler = new HostRequestHandler(queue);

        byte[] response = await handler.HandleAsync(
            ValidPayload(Guid.NewGuid()),
            PeerAuthorization.Rejected,
            TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(response);
        Assert.False(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal("unauthorizedPeer", document.RootElement.GetProperty("reason").GetString());
        Assert.Equal(0, queue.Count);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"schemaVersion\":999,\"requestId\":\"11111111-1111-1111-1111-111111111111\",\"action\":\"convertUsingDefault\",\"files\":[\"C:\\\\a.wav\"]}")]
    public async Task MalformedOrUnsupportedRequestDoesNotEnqueue(string json)
    {
        var queue = new HostJobQueue(capacity: 4);
        var handler = new HostRequestHandler(queue);

        byte[] response = await handler.HandleAsync(
            Encoding.UTF8.GetBytes(json),
            PeerAuthorization.ExpectedUser,
            TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(response);
        Assert.False(document.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal("invalidRequest", document.RootElement.GetProperty("reason").GetString());
        Assert.Equal(0, queue.Count);
    }

    private static byte[] ValidPayload(Guid requestId)
    {
        var request = new ConversionRequest(
            SchemaVersions.Current,
            requestId,
            ConversionAction.ConvertUsingDefault,
            [@"C:\input\sample.wav"],
            targetFormat: null,
            presetId: null);
        return Encoding.UTF8.GetBytes(ContractJson.Serialize(request));
    }
}
