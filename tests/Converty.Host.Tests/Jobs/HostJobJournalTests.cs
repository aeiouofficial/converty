using System.Text;
using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Jobs;
using Converty.Host.Jobs;

namespace Converty.Host.Tests.Jobs;

public sealed class HostJobJournalTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "converty-journal-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void CommitThenLoadRestoresQueuedAndCancelledJobs()
    {
        Directory.CreateDirectory(_directory);
        string path = Path.Combine(_directory, "jobs.json");
        var journal = new HostJobJournal(path);
        var queued = Snapshot(Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Queued, null);
        var cancelled = Snapshot(Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Cancelled, "Cancelled before execution.");

        journal.Commit([queued, cancelled]);
        IReadOnlyList<JobStatusSnapshot> loaded = journal.LoadForRecovery();

        Assert.Equal(2, loaded.Count);
        Assert.Contains(loaded, item => item.JobId == queued.JobId && item.State == ConversionJobState.Queued);
        Assert.Contains(loaded, item => item.JobId == cancelled.JobId && item.State == ConversionJobState.Cancelled);
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void CommitReplacesPriorGenerationAndOrphanTempCannotOverrideCommittedState()
    {
        Directory.CreateDirectory(_directory);
        string path = Path.Combine(_directory, "jobs.json");
        var journal = new HostJobJournal(path);
        var first = Snapshot(Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Queued, null);
        var second = Snapshot(Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Cancelled, "done");
        journal.Commit([first]);
        journal.Commit([second]);
        File.WriteAllText(path + ".tmp", "{\"schemaVersion\":999}", Encoding.UTF8);

        IReadOnlyList<JobStatusSnapshot> loaded = journal.LoadForRecovery();

        JobStatusSnapshot only = Assert.Single(loaded);
        Assert.Equal(second.JobId, only.JobId);
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void InFlightStateBecomesFailedOnRecovery()
    {
        Directory.CreateDirectory(_directory);
        string path = Path.Combine(_directory, "jobs.json");
        var journal = new HostJobJournal(path);
        var converting = Snapshot(Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Converting, null);
        journal.Commit([converting]);

        JobStatusSnapshot recovered = Assert.Single(journal.LoadForRecovery());

        Assert.Equal(ConversionJobState.Failed, recovered.State);
        Assert.Equal("Interrupted by Host restart.", recovered.Message);
    }

    [Theory]
    [InlineData("{\"schemaVersion\":2,\"jobs\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"jobs\":[],\"unknown\":true}")]
    [InlineData("{\"schemaVersion\":1,\"schemaVersion\":1,\"jobs\":[]}")]
    public void StrictLoadRejectsUnsupportedUnknownAndDuplicateMembers(string json)
    {
        Directory.CreateDirectory(_directory);
        string path = Path.Combine(_directory, "jobs.json");
        File.WriteAllText(path, json, Encoding.UTF8);
        var journal = new HostJobJournal(path);

        Assert.Throws<InvalidDataException>(() => journal.LoadForRecovery());
    }

    [Fact]
    public void DuplicateJobOrRequestIdsAreRejected()
    {
        Directory.CreateDirectory(_directory);
        string path = Path.Combine(_directory, "jobs.json");
        Guid sharedRequest = Guid.NewGuid();
        string json = $$"""
            {"schemaVersion":1,"jobs":[
              {"schemaVersion":1,"jobId":"{{Guid.NewGuid():D}}","requestId":"{{sharedRequest:D}}","state":"queued","progress":null,"message":null},
              {"schemaVersion":1,"jobId":"{{Guid.NewGuid():D}}","requestId":"{{sharedRequest:D}}","state":"queued","progress":null,"message":null}
            ]}
            """;
        File.WriteAllText(path, json, Encoding.UTF8);
        var journal = new HostJobJournal(path);

        Assert.Throws<InvalidDataException>(() => journal.LoadForRecovery());
    }

    [Fact]
    public void QueueRejectsPersistenceFailureWithoutPublishingMutation()
    {
        var queue = new HostJobQueue(capacity: 2, journal: new ThrowingJournal());

        JobAdmissionResult result = queue.TryEnqueue(CreateRequest(Guid.NewGuid()));

        Assert.False(result.Accepted);
        Assert.Equal(JobAdmissionRejection.PersistenceFailure, result.Rejection);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void QueueRestoresJournalBeforeAcceptingNewWork()
    {
        var existing = Snapshot(Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Queued, null);
        var journal = new MemoryJournal([existing]);
        var queue = new HostJobQueue(capacity: 2, journal);

        Assert.Equal(1, queue.Count);
        Assert.True(queue.TryGet(existing.JobId, out JobStatusSnapshot? restored));
        Assert.Equal(existing.RequestId, restored!.RequestId);
        Assert.Equal(JobAdmissionRejection.DuplicateRequest, queue.TryEnqueue(CreateRequest(existing.RequestId)).Rejection);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static JobStatusSnapshot Snapshot(Guid jobId, Guid requestId, ConversionJobState state, string? message) =>
        new(SchemaVersions.Current, jobId, requestId, state, progress: null, message);

    private static ConversionRequest CreateRequest(Guid requestId) =>
        new(SchemaVersions.Current, requestId, ConversionAction.ConvertUsingDefault, [@"C:\input\sample.wav"], null, null);

    private sealed class ThrowingJournal : IHostJobJournal
    {
        public IReadOnlyList<JobStatusSnapshot> LoadForRecovery() => [];
        public void Commit(IReadOnlyCollection<JobStatusSnapshot> snapshots) => throw new IOException("disk failure");
    }

    private sealed class MemoryJournal(IReadOnlyList<JobStatusSnapshot> initial) : IHostJobJournal
    {
        private IReadOnlyList<JobStatusSnapshot> _items = initial;
        public IReadOnlyList<JobStatusSnapshot> LoadForRecovery() => _items;
        public void Commit(IReadOnlyCollection<JobStatusSnapshot> snapshots) => _items = snapshots.ToArray();
    }
}
