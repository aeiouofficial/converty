using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;

namespace Converty.Host.Jobs;

public sealed class HostJobQueue
{
    private readonly object _gate = new();
    private readonly int _capacity;
    private readonly Dictionary<Guid, JobStatusSnapshot> _jobs = new();
    private readonly Dictionary<Guid, Guid> _requestToJob = new();

    public HostJobQueue(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Queue capacity must be at least one.");
        }

        _capacity = capacity;
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

            status = new JobStatusSnapshot(
                SchemaVersions.Current,
                current.JobId,
                current.RequestId,
                ConversionJobState.Cancelled,
                progress: current.Progress,
                message: "Cancelled before execution.");
            _jobs[jobId] = status;
            return true;
        }
    }
}
