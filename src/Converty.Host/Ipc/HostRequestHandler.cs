using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Host.Jobs;
using Converty.Serialization;

namespace Converty.Host.Ipc;

public sealed class HostRequestHandler
{
    public const int MaximumRequestBytes = 1024 * 1024;

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private readonly HostJobQueue _queue;

    public HostRequestHandler(HostJobQueue queue)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    public Task<byte[]> HandleAsync(
        ReadOnlyMemory<byte> payload,
        PeerAuthorization authorization,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (authorization != PeerAuthorization.ExpectedUser)
        {
            return Task.FromResult(SerializeResponse(accepted: false, Guid.Empty, "unauthorizedPeer"));
        }

        if (payload.Length is < 1 or > MaximumRequestBytes)
        {
            return Task.FromResult(SerializeResponse(accepted: false, Guid.Empty, "invalidRequest"));
        }

        ConversionRequest request;
        try
        {
            string json = StrictUtf8.GetString(payload.Span);
            request = ContractJson.DeserializeConversionRequest(json);
        }
        catch (Exception error) when (error is JsonException or DecoderFallbackException)
        {
            return Task.FromResult(SerializeResponse(accepted: false, Guid.Empty, "invalidRequest"));
        }

        JobAdmissionResult admission = _queue.TryEnqueue(request);
        if (admission.Accepted)
        {
            return Task.FromResult(SerializeResponse(accepted: true, admission.JobId, reason: null));
        }

        string reason = admission.Rejection switch
        {
            JobAdmissionRejection.DuplicateRequest => "duplicateRequest",
            JobAdmissionRejection.QueueFull => "queueFull",
            JobAdmissionRejection.PersistenceFailure => "persistenceFailure",
            _ => "rejected",
        };
        return Task.FromResult(SerializeResponse(accepted: false, Guid.Empty, reason));
    }

    private static byte[] SerializeResponse(bool accepted, Guid jobId, string? reason) =>
        JsonSerializer.SerializeToUtf8Bytes(new AdmissionResponse
        {
            SchemaVersion = SchemaVersions.Current,
            Accepted = accepted,
            JobId = accepted ? jobId.ToString("D") : null,
            Reason = reason,
        });

    private sealed class AdmissionResponse
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; init; }

        [JsonPropertyName("accepted")]
        public bool Accepted { get; init; }

        [JsonPropertyName("jobId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobId { get; init; }

        [JsonPropertyName("reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Reason { get; init; }
    }
}
