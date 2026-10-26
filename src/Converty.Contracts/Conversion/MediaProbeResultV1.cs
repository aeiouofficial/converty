namespace Converty.Contracts.Conversion;

public sealed class MediaProbeResultV1
{
    public const int SchemaVersion = 1;

    private MediaProbeResultV1(
        MediaProbeStatus status,
        MediaProbeFactsV1? facts,
        MediaProbeFailureReason failureReason)
    {
        Status = status;
        Facts = facts;
        FailureReason = failureReason;
    }

    public MediaProbeStatus Status { get; }
    public MediaProbeFactsV1? Facts { get; }
    public MediaProbeFailureReason FailureReason { get; }

    public static MediaProbeResultV1 Success(MediaProbeFactsV1 facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        return new MediaProbeResultV1(MediaProbeStatus.Success, facts, MediaProbeFailureReason.None);
    }

    public static MediaProbeResultV1 Failure(MediaProbeFailureReason failureReason)
    {
        if (!Enum.IsDefined(failureReason) || failureReason == MediaProbeFailureReason.None)
        {
            throw new ArgumentOutOfRangeException(nameof(failureReason));
        }

        return new MediaProbeResultV1(MediaProbeStatus.Failure, null, failureReason);
    }
}
