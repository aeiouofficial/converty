using System.Text;
using System.Text.Json;
using Converty.Host.Ipc;
using Converty.Host.Jobs;
using Converty.Ipc.Protocol;

namespace Converty.Host.Tests.Ipc;

public sealed class IpcFuzzCorpusTests
{
    [Fact]
    public async Task CheckedInCorpusRejectsWithoutQueueMutation()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "fuzz", "ipc", "v1", "corpus.json");
        string raw = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
        using JsonDocument document = JsonDocument.Parse(raw);

        JsonElement cases = document.RootElement.GetProperty("cases");
        Assert.Equal(7, cases.GetArrayLength());

        foreach (JsonElement fuzzCase in cases.EnumerateArray())
        {
            string id = fuzzCase.GetProperty("id").GetString()!;
            string kind = fuzzCase.GetProperty("kind").GetString()!;
            string data = fuzzCase.GetProperty("data").GetString()!;

            if (string.Equals(kind, "frameHex", StringComparison.Ordinal))
            {
                byte[] frameBytes = Convert.FromHexString(data);
                await using var stream = new MemoryStream(frameBytes, writable: false);
                ProtocolException error = await Assert.ThrowsAsync<ProtocolException>(async () =>
                    await ProtocolFrameCodec.ReadAsync(stream, TestContext.Current.CancellationToken));

                string expectedName = fuzzCase.GetProperty("expectProtocolError").GetString()!;
                Assert.True(Enum.TryParse(expectedName, ignoreCase: false, out ProtocolErrorCode expected), id);
                Assert.Equal(expected, error.ErrorCode);
                continue;
            }

            Assert.Equal("requestText", kind);
            var queue = new HostJobQueue(capacity: 4);
            var handler = new HostRequestHandler(queue);
            byte[] response = await handler.HandleAsync(
                Encoding.UTF8.GetBytes(data),
                PeerAuthorization.ExpectedUser,
                TestContext.Current.CancellationToken);

            using JsonDocument responseDocument = JsonDocument.Parse(response);
            Assert.False(responseDocument.RootElement.GetProperty("accepted").GetBoolean());
            Assert.Equal(
                fuzzCase.GetProperty("expectReason").GetString(),
                responseDocument.RootElement.GetProperty("reason").GetString());
            Assert.Equal(0, queue.Count);
        }
    }
}
