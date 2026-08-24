namespace FileConvert.Contracts.Jobs;

/// <summary>
/// Bounded status data only. Persistence/journaling, transport, and process control belong to later modules.
/// </summary>
public sealed class JobStatusSnapshot
{
    public const int MaximumMessageLength = 1024;

    public JobStatusSnapshot(
        int schemaVersion,
        Guid jobId,
        Guid requestId,
        ConversionJobState state,
        double? progress,
        string? message)
    {
        if (schemaVersion != SchemaVersions.Current)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Unsupported job status schema version.");
        }

        if (jobId == Guid.Empty)
        {
            throw new ArgumentException("Job ID must not be empty.", nameof(jobId));
        }

        if (requestId == Guid.Empty)
        {
            throw new ArgumentException("Request ID must not be empty.", nameof(requestId));
        }

        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), "Unsupported job state.");
        }

        if (progress is { } value && (!double.IsFinite(value) || value is < 0.0 or > 1.0))
        {
            throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be finite and between 0 and 1.");
        }

        if (message is not null && (string.IsNullOrWhiteSpace(message) || message.Length > MaximumMessageLength))
        {
            throw new ArgumentException($"Message must be 1-{MaximumMessageLength} non-whitespace characters when supplied.", nameof(message));
        }

        SchemaVersion = schemaVersion;
        JobId = jobId;
        RequestId = requestId;
        State = state;
        Progress = progress;
        Message = message?.Trim();
    }

    public int SchemaVersion { get; }
    public Guid JobId { get; }
    public Guid RequestId { get; }
    public ConversionJobState State { get; }
    public double? Progress { get; }
    public string? Message { get; }
}
