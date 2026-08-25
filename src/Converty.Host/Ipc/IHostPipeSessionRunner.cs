namespace Converty.Host.Ipc;

public interface IHostPipeSessionRunner
{
    Task<HostPipeSessionResult> RunSingleConnectionAsync(CancellationToken cancellationToken);
}
