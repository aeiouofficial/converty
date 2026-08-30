namespace Converty.Contracts.Jobs;

public enum JobControlFailureReason
{
    JobNotFound = 0,
    NotCancellable = 1,
    PersistenceFailure = 2,
}
