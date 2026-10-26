namespace Converty.Security.Workers;

public sealed class WorkerStandardOutputLimitExceededException : IOException
{
    public WorkerStandardOutputLimitExceededException(long maximumBytes, long observedBytes)
        : base($"Converty worker standard output exceeded the configured {maximumBytes}-byte limit.")
    {
        MaximumBytes = maximumBytes;
        ObservedBytes = observedBytes;
    }

    public long MaximumBytes { get; }

    public long ObservedBytes { get; }
}
