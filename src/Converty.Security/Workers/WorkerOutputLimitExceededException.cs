namespace Converty.Security.Workers;

public sealed class WorkerOutputLimitExceededException : IOException
{
    public WorkerOutputLimitExceededException(long maximumOutputBytes, long observedOutputGrowthBytes)
        : base("Converty worker exceeded the configured private-staging output byte limit.")
    {
        MaximumOutputBytes = maximumOutputBytes;
        ObservedOutputGrowthBytes = observedOutputGrowthBytes;
    }

    public long MaximumOutputBytes { get; }

    public long ObservedOutputGrowthBytes { get; }
}
