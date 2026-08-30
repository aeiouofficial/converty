using System.Globalization;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;
using Converty.Ipc.Protocol;
using Converty.Security.Ipc;
using Converty.Serialization;

namespace Converty.Bridge.Ipc;

[SupportedOSPlatform("windows")]
public sealed class BridgeClient : IBridgeRequestClient, IBridgeJobControlClient
{
    public static readonly TimeSpan MaximumConnectTimeout = TimeSpan.FromSeconds(30);

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly HashSet<string> ResponseMembers = new(StringComparer.Ordinal)
    {
        "schemaVersion",
        "accepted",
        "jobId",
        "reason",
    };

    private readonly TimeSpan _connectTimeout;
    private readonly IConnectedServerIdentityVerifier _serverIdentityVerifier;

    public BridgeClient(
        string pipeName,
        TimeSpan connectTimeout,
        IConnectedServerIdentityVerifier serverIdentityVerifier)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new ArgumentException("Pipe name is required.", nameof(pipeName));
        }

        if (connectTimeout <= TimeSpan.Zero || connectTimeout > MaximumConnectTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(connectTimeout),
                $"Connect timeout must be greater than zero and at most {MaximumConnectTimeout.TotalSeconds} seconds.");
        }

        PipeName = pipeName;
        _connectTimeout = connectTimeout;
        _serverIdentityVerifier = serverIdentityVerifier ?? throw new ArgumentNullException(nameof(serverIdentityVerifier));
    }

    public string PipeName { get; }

    public static BridgeClient ForCurrentUser(
        TimeSpan connectTimeout,
        IConnectedServerIdentityVerifier serverIdentityVerifier)
    {
        SecurityIdentifier userSid = WindowsIdentity.GetCurrent().User
            ?? throw new InvalidOperationException("Current Windows identity has no user SID.");
        return new BridgeClient(PipeEndpointName.ForUser(userSid), connectTimeout, serverIdentityVerifier);
    }

    public async Task<BridgeSubmissionResult> SubmitAsync(
        ConversionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        byte[] payload = Encoding.UTF8.GetBytes(ContractJson.Serialize(request));
        ProtocolFrame response = await ExchangeAsync(payload, cancellationToken);
        return ParseResponse(response.Payload);
    }

    public Task<JobControlResponse> GetStatusAsync(
        Guid jobId,
        CancellationToken cancellationToken = default) =>
        SendJobControlAsync(JobControlOperation.Status, jobId, cancellationToken);

    public Task<JobControlResponse> CancelAsync(
        Guid jobId,
        CancellationToken cancellationToken = default) =>
        SendJobControlAsync(JobControlOperation.Cancel, jobId, cancellationToken);

    private async Task<JobControlResponse> SendJobControlAsync(
        JobControlOperation operation,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var request = new JobControlRequest(SchemaVersions.Current, operation, jobId);
        byte[] payload = Encoding.UTF8.GetBytes(ContractJson.Serialize(request));
        ProtocolFrame responseFrame = await ExchangeAsync(payload, cancellationToken);

        JobControlResponse response;
        try
        {
            string json = StrictUtf8.GetString(responseFrame.Payload.Span);
            response = ContractJson.DeserializeJobControlResponse(json);
        }
        catch (Exception error) when (error is JsonException or DecoderFallbackException)
        {
            throw new InvalidDataException("Bridge job-control response is not valid strict JSON.", error);
        }

        ValidateJobControlResponse(operation, jobId, response);
        return response;
    }

    private async Task<ProtocolFrame> ExchangeAsync(
        byte[] payload,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payload);

        await using var pipe = new NamedPipeClientStream(
            ".",
            PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous,
            TokenImpersonationLevel.Impersonation);

        try
        {
            await pipe.ConnectAsync(_connectTimeout, cancellationToken);
        }
        catch (TimeoutException error)
        {
            throw new BridgeHostUnavailableException("Converty Host did not accept the pipe connection before the connect timeout.", error);
        }
        catch (IOException error)
        {
            throw new BridgeHostUnavailableException("Converty Host pipe connection is unavailable.", error);
        }

        _serverIdentityVerifier.VerifyConnectedServer(pipe);

        await BoundedProtocolFrameIo.WriteAndFlushAsync(pipe, payload, _connectTimeout, cancellationToken);
        return await BoundedProtocolFrameIo.ReadAsync(pipe, _connectTimeout, cancellationToken);
    }

    private static void ValidateJobControlResponse(
        JobControlOperation expectedOperation,
        Guid expectedJobId,
        JobControlResponse response)
    {
        if (response.Operation != expectedOperation)
        {
            throw new InvalidDataException("Bridge job-control response operation does not match the request.");
        }

        if (response.JobId != expectedJobId)
        {
            throw new InvalidDataException("Bridge job-control response job ID does not match the request.");
        }

        if (expectedOperation == JobControlOperation.Status)
        {
            if (!response.Succeeded && response.Reason != JobControlFailureReason.JobNotFound)
            {
                throw new InvalidDataException("Status response contains a cancellation-only failure reason.");
            }

            return;
        }

        if (response.Succeeded)
        {
            if (response.Status?.State != ConversionJobState.Cancelled)
            {
                throw new InvalidDataException("Successful cancel response must contain Cancelled status.");
            }

            return;
        }

        switch (response.Reason)
        {
            case JobControlFailureReason.JobNotFound:
                return;
            case JobControlFailureReason.PersistenceFailure:
                if (response.Status?.State != ConversionJobState.Queued)
                {
                    throw new InvalidDataException("Cancel persistenceFailure response must contain Queued status.");
                }

                return;
            case JobControlFailureReason.NotCancellable:
                if (response.Status is null || response.Status.State == ConversionJobState.Queued)
                {
                    throw new InvalidDataException("Cancel notCancellable response must contain non-Queued status.");
                }

                return;
            default:
                throw new InvalidDataException("Cancel response contains an unsupported failure reason.");
        }
    }

    private static BridgeSubmissionResult ParseResponse(ReadOnlyMemory<byte> payload)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(payload);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("Bridge response root must be an object.");
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            int? schemaVersion = null;
            bool? accepted = null;
            string? jobIdText = null;
            string? reason = null;

            foreach (JsonProperty property in root.EnumerateObject())
            {
                if (!ResponseMembers.Contains(property.Name) || !seen.Add(property.Name))
                {
                    throw new InvalidDataException($"Unexpected or duplicate Bridge response member '{property.Name}'.");
                }

                switch (property.Name)
                {
                    case "schemaVersion":
                        if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out int parsedVersion))
                        {
                            throw new InvalidDataException("Bridge response schemaVersion must be an integer.");
                        }

                        schemaVersion = parsedVersion;
                        break;
                    case "accepted":
                        if (property.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                        {
                            throw new InvalidDataException("Bridge response accepted must be a boolean.");
                        }

                        accepted = property.Value.GetBoolean();
                        break;
                    case "jobId":
                        jobIdText = property.Value.ValueKind switch
                        {
                            JsonValueKind.String => property.Value.GetString(),
                            JsonValueKind.Null => null,
                            _ => throw new InvalidDataException("Bridge response jobId must be a string or null."),
                        };
                        break;
                    case "reason":
                        reason = property.Value.ValueKind switch
                        {
                            JsonValueKind.String => property.Value.GetString(),
                            JsonValueKind.Null => null,
                            _ => throw new InvalidDataException("Bridge response reason must be a string or null."),
                        };
                        break;
                }
            }

            if (schemaVersion != SchemaVersions.Current)
            {
                string versionText = schemaVersion?.ToString(CultureInfo.InvariantCulture) ?? "<missing>";
                throw new InvalidDataException($"Unsupported Bridge response schema version {versionText}.");
            }

            if (accepted is null)
            {
                throw new InvalidDataException("Bridge response is missing accepted.");
            }

            if (accepted.Value)
            {
                if (!Guid.TryParseExact(jobIdText, "D", out Guid jobId) || jobId == Guid.Empty || reason is not null)
                {
                    throw new InvalidDataException("Accepted Bridge response must contain one non-empty jobId and no reason.");
                }

                return new BridgeSubmissionResult(true, jobId, null);
            }

            if (jobIdText is not null || string.IsNullOrWhiteSpace(reason) || reason.Length > 128)
            {
                throw new InvalidDataException("Rejected Bridge response must contain a bounded reason and no jobId.");
            }

            return new BridgeSubmissionResult(false, null, reason);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Bridge response is not valid strict JSON.", error);
        }
    }
}
