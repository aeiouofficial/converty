namespace Converty.Contracts.Jobs;

public sealed class JobControlRequest
{
    public JobControlRequest(int schemaVersion, JobControlOperation operation, Guid jobId)
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

        SchemaVersion = schemaVersion;
        Operation = operation;
        JobId = jobId;
    }

    public int SchemaVersion { get; }
    public JobControlOperation Operation { get; }
    public Guid JobId { get; }
}
