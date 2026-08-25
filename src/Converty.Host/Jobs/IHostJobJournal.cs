using Converty.Contracts.Jobs;

namespace Converty.Host.Jobs;

public interface IHostJobJournal
{
    IReadOnlyList<JobStatusSnapshot> LoadForRecovery();

    void Commit(IReadOnlyCollection<JobStatusSnapshot> snapshots);
}
