namespace Converty.Contracts.Jobs;

public sealed class JobControlResponse
{
    public JobControlResponse(
        int schemaVersion,
        JobControlOperation operation,
        Guid jobId,
        bool succeeded,
        JobStatusSnapshot? status,
        JobControlFailureReason? reason)
    {
        if (schemaVersion != SchemaVersions.Current)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Unsupported job-control schema version.");
        }

        if (!Enum.IsDefined(operation))
        {
            throw new ArgumentOutOfRangeException(nameof(operation), "Unsupported job-control operation.");
        }

        if (jobId == Guid.Empty)
        {
            throw new ArgumentException("Job ID must not be empty.", nameof(jobId));
        }

        if (succeeded)
        {
            if (status is null || reason is not null || status.JobId != jobId)
            {
                throw new ArgumentException("Successful job-control responses require matching status and no reason.");
            }
        }
        else
        {
            if (reason is null || !Enum.IsDefined(reason.Value))
            {
                throw new ArgumentException("Failed job-control responses require a defined reason.", nameof(reason));
            }

            bool statusRequired = reason is JobControlFailureReason.NotCancellable
                or JobControlFailureReason.PersistenceFailure;
            if ((statusRequired && status is null) ||
                (!statusRequired && status is not null) ||
                (status is not null && status.JobId != jobId))
            {
                throw new ArgumentException("Job-control failure status does not match its reason/job ID.", nameof(status));
            }
        }

        SchemaVersion = schemaVersion;
        Operation = operation;
        JobId = jobId;
        Succeeded = succeeded;
        Status = status;
        Reason = reason;
    }

    public int SchemaVersion { get; }
    public JobControlOperation Operation { get; }
    public Guid JobId { get; }
    public bool Succeeded { get; }
    public JobStatusSnapshot? Status { get; }
    public JobControlFailureReason? Reason { get; }
}
