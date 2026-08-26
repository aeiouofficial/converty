namespace Converty.Security.Workers;

public interface IWorkerProcessLauncher
{
    Task<WorkerProcessResult> ExecuteAsync(
        WorkerProcessLaunchRequest request,
        CancellationToken cancellationToken = default);
}
