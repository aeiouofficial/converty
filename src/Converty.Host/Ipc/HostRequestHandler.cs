using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;
using Converty.Host.Jobs;
using Converty.Serialization;

namespace Converty.Host.Ipc;

public sealed class HostRequestHandler
{
    public const int MaximumRequestBytes = 1024 * 1024;

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly JsonDocumentOptions ClassificationDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 32,
    };

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

        try
        {
            string json = StrictUtf8.GetString(payload.Span);
            if (IsJobControlRequest(json))
            {
                JobControlRequest controlRequest = ContractJson.DeserializeJobControlRequest(json);
                return Task.FromResult(HandleJobControl(controlRequest));
            }

            ConversionRequest request = ContractJson.DeserializeConversionRequest(json);
            return Task.FromResult(HandleAdmission(request));
        }
        catch (Exception error) when (error is JsonException or DecoderFallbackException)
        {
            return Task.FromResult(SerializeResponse(accepted: false, Guid.Empty, "invalidRequest"));
        }
    }

    private byte[] HandleAdmission(ConversionRequest request)
    {
        JobAdmissionResult admission = _queue.TryEnqueue(request);
        if (admission.Accepted)
        {
            return SerializeResponse(accepted: true, admission.JobId, reason: null);
        }

        if (admission.Rejection == JobAdmissionRejection.DuplicateRequest &&
            _queue.TryGetByRequestId(request.RequestId, out JobStatusSnapshot? existing) &&
            existing is not null)
        {
            return SerializeResponse(accepted: true, existing.JobId, reason: null);
        }

        string reason = admission.Rejection switch
        {
            JobAdmissionRejection.DuplicateRequest => "duplicateRequest",
            JobAdmissionRejection.QueueFull => "queueFull",
            JobAdmissionRejection.PersistenceFailure => "persistenceFailure",
            _ => "rejected",
        };
        return SerializeResponse(accepted: false, Guid.Empty, reason);
    }

    private byte[] HandleJobControl(JobControlRequest request) => request.Operation switch
    {
        JobControlOperation.Status => HandleStatus(request),
        JobControlOperation.Cancel => HandleCancel(request),
        _ => throw new InvalidOperationException("Unsupported validated job-control operation."),
    };

    private byte[] HandleStatus(JobControlRequest request)
    {
        if (_queue.TryGet(request.JobId, out JobStatusSnapshot? status))
        {
            return SerializeJobControl(new JobControlResponse(
                SchemaVersions.Current,
                request.Operation,
                request.JobId,
                succeeded: true,
                status,
                reason: null));
        }

        return SerializeJobControl(new JobControlResponse(
            SchemaVersions.Current,
            request.Operation,
            request.JobId,
            succeeded: false,
            status: null,
            JobControlFailureReason.JobNotFound));
    }

    private byte[] HandleCancel(JobControlRequest request)
    {
        bool cancelled = _queue.TryCancel(request.JobId, out JobStatusSnapshot? status);
        if (cancelled)
        {
            if (status is null || status.State != ConversionJobState.Cancelled)
            {
                throw new InvalidOperationException("Host queue returned an invalid successful cancellation state.");
            }

            return SerializeJobControl(new JobControlResponse(
                SchemaVersions.Current,
                request.Operation,
                request.JobId,
                succeeded: true,
                status,
                reason: null));
        }

        if (status is null)
        {
            return SerializeJobControl(new JobControlResponse(
                SchemaVersions.Current,
                request.Operation,
                request.JobId,
                succeeded: false,
                status: null,
                JobControlFailureReason.JobNotFound));
        }

        JobControlFailureReason reason = status.State == ConversionJobState.Queued
            ? JobControlFailureReason.PersistenceFailure
            : JobControlFailureReason.NotCancellable;

        return SerializeJobControl(new JobControlResponse(
            SchemaVersions.Current,
            request.Operation,
            request.JobId,
            succeeded: false,
            status,
            reason));
    }

    private static bool IsJobControlRequest(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json, ClassificationDocumentOptions);
        return document.RootElement.ValueKind == JsonValueKind.Object &&
            document.RootElement.TryGetProperty("operation", out _);
    }

    private static byte[] SerializeJobControl(JobControlResponse response) =>
        Encoding.UTF8.GetBytes(ContractJson.Serialize(response));

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
