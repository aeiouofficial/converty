using System.Diagnostics.CodeAnalysis;
using System.Security;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;

namespace Converty.Host.Jobs;

[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "The B2 contract intentionally exposes this bounded Host job queue as HostJobQueue.")]
public sealed class HostJobQueue
{
    private readonly object _gate = new();
    private readonly int _capacity;
    private readonly IHostJobJournal? _journal;
    private readonly Dictionary<Guid, JobStatusSnapshot> _jobs = new();
    private readonly Dictionary<Guid, Guid> _requestToJob = new();

    public HostJobQueue(int capacity, IHostJobJournal? journal = null)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be at least one.");
        }

        _capacity = capacity;
        _journal = journal;
        if (_journal is null)
        {
            return;
        }

        IReadOnlyList<JobStatusSnapshot> restored = _journal.LoadForRecovery();
        if (restored.Count > _capacity)
        {
            throw new InvalidDataException("Recovered Host job state exceeds configured queue capacity.");
        }

        foreach (JobStatusSnapshot status in restored)
        {
            if (!_jobs.TryAdd(status.JobId, status) || !_requestToJob.TryAdd(status.RequestId, status.JobId))
            {
                throw new InvalidDataException("Recovered Host job state contains duplicate job or request IDs.");
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _jobs.Count;
            }
        }
    }

    public JobAdmissionResult TryEnqueue(ConversionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_gate)
        {
            if (_requestToJob.ContainsKey(request.RequestId))
            {
                return JobAdmissionResult.Reject(JobAdmissionRejection.DuplicateRequest);
            }

            if (_jobs.Count >= _capacity)
            {
                return JobAdmissionResult.Reject(JobAdmissionRejection.QueueFull);
            }

            Guid jobId = Guid.NewGuid();
            var status = new JobStatusSnapshot(
                SchemaVersions.Current,
                jobId,
                request.RequestId,
                ConversionJobState.Queued,
                progress: null,
                message: null);

            if (!TryPersistWith(status, replacementJobId: null))
            {
                return JobAdmissionResult.Reject(JobAdmissionRejection.PersistenceFailure);
            }

            _jobs.Add(jobId, status);
            _requestToJob.Add(request.RequestId, jobId);
            return JobAdmissionResult.Accept(jobId);
        }
    }

    public bool TryGet(Guid jobId, out JobStatusSnapshot? status)
    {
        if (jobId == Guid.Empty)
        {
            status = null;
            return false;
        }

        lock (_gate)
        {
            return _jobs.TryGetValue(jobId, out status);
        }
    }

    public bool TryCancel(Guid jobId, out JobStatusSnapshot? status)
    {
        if (jobId == Guid.Empty)
        {
            status = null;
            return false;
        }

        lock (_gate)
        {
            if (!_jobs.TryGetValue(jobId, out JobStatusSnapshot? current) || current.State != ConversionJobState.Queued)
            {
                status = current;
                return false;
            }

            var cancelled = new JobStatusSnapshot(
                SchemaVersions.Current,
                current.JobId,
                current.RequestId,
                ConversionJobState.Cancelled,
                progress: current.Progress,
                message: "Cancelled before execution.");

            if (!TryPersistWith(cancelled, current.JobId))
            {
                status = current;
                return false;
            }

            _jobs[jobId] = cancelled;
            status = cancelled;
            return true;
        }
    }

    private bool TryPersistWith(JobStatusSnapshot candidate, Guid? replacementJobId)
    {
        if (_journal is null)
        {
            return true;
        }

        var snapshots = new List<JobStatusSnapshot>(_jobs.Count + (replacementJobId is null ? 1 : 0));
        foreach (JobStatusSnapshot existing in _jobs.Values)
        {
            if (replacementJobId == existing.JobId)
            {
                snapshots.Add(candidate);
            }
            else
            {
                snapshots.Add(existing);
            }
        }

        if (replacementJobId is null)
        {
            snapshots.Add(candidate);
        }

        try
        {
            _journal.Commit(snapshots);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or SecurityException)
        {
            return false;
        }
    }
}
