using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Host.Ipc;
using Converty.Host.Jobs;
using Converty.Security.Ipc;

namespace Converty.Host.Runtime;

public enum HostRuntimeResult
{
    Stopped = 0,
    AlreadyRunning = 1,
}

[SupportedOSPlatform("windows")]
public sealed class HostRuntime
{
    private readonly SecurityIdentifier _userSid;
    private readonly Func<HostJobQueue> _queueFactory;
    private readonly Func<HostJobQueue, IHostPipeSessionRunner> _sessionFactory;

    public HostRuntime(
        SecurityIdentifier userSid,
        Func<HostJobQueue> queueFactory,
        Func<HostJobQueue, IHostPipeSessionRunner> sessionFactory)
    {
        _userSid = userSid ?? throw new ArgumentNullException(nameof(userSid));
        _queueFactory = queueFactory ?? throw new ArgumentNullException(nameof(queueFactory));
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
    }

    public static HostRuntime CreateForCurrentUser(string journalPath, int queueCapacity)
    {
        if (string.IsNullOrWhiteSpace(journalPath))
        {
            throw new ArgumentException("Journal path is required.", nameof(journalPath));
        }

        if (queueCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(queueCapacity), "Queue capacity must be at least one.");
        }

        using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query);
        SecurityIdentifier userSid = identity.User
            ?? throw new InvalidOperationException("Current Windows identity has no user SID.");
        string pipeName = PipeEndpointName.ForUser(userSid);
        var peerValidator = new ConnectedPeerValidator(new WindowsConnectedPeerIdentityReader());

        return new HostRuntime(
            userSid,
            () => new HostJobQueue(queueCapacity, new HostJobJournal(journalPath)),
            queue => new HostPipeServer(
                pipeName,
                userSid,
                peerValidator,
                new HostRequestHandler(queue)));
    }

    public async Task<HostRuntimeResult> RunAsync(CancellationToken cancellationToken = default)
    {
        if (!HostSingleInstanceLease.TryAcquire(_userSid, out HostSingleInstanceLease? lease))
        {
            return HostRuntimeResult.AlreadyRunning;
        }

        using (lease)
        {
            try
            {
                HostJobQueue queue = _queueFactory();
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    IHostPipeSessionRunner session = _sessionFactory(queue)
                        ?? throw new InvalidOperationException("Host session factory returned null.");
                    await session.RunSingleConnectionAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return HostRuntimeResult.Stopped;
            }
        }
    }
}
