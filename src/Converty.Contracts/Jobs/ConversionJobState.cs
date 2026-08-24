namespace Converty.Contracts.Jobs;

public enum ConversionJobState
{
    Queued = 0,
    Probing = 1,
    Planning = 2,
    Staging = 3,
    Converting = 4,
    Validating = 5,
    Committing = 6,
    Completed = 7,
    Failed = 8,
    Cancelled = 9,
    Rejected = 10,
}
