namespace Converty.Host.Jobs;

public enum JobAdmissionRejection
{
    None = 0,
    DuplicateRequest = 1,
    QueueFull = 2,
    PersistenceFailure = 3,
}

public readonly record struct JobAdmissionResult(Guid JobId, JobAdmissionRejection Rejection)
{
    public bool Accepted => Rejection == JobAdmissionRejection.None;

    public static JobAdmissionResult Accept(Guid jobId) => new(jobId, JobAdmissionRejection.None);

    public static JobAdmissionResult Reject(JobAdmissionRejection rejection) => new(Guid.Empty, rejection);
}
