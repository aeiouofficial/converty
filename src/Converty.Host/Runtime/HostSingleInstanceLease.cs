using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Security.Ipc;

namespace Converty.Host.Runtime;

[SupportedOSPlatform("windows")]
public sealed class HostSingleInstanceLease : IDisposable
{
    private const string EndpointPrefix = "converty.";
    private readonly Mutex _mutex;
    private bool _disposed;

    private HostSingleInstanceLease(Mutex mutex)
    {
        _mutex = mutex;
    }

    public static string NameForUser(SecurityIdentifier userSid)
    {
        ArgumentNullException.ThrowIfNull(userSid);

        string endpointName = PipeEndpointName.ForUser(userSid);
        if (!endpointName.StartsWith(EndpointPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The current endpoint-name format is not valid for Host instance identity.");
        }

        return @"Local\Converty.Host." + endpointName[EndpointPrefix.Length..];
    }

    public static bool TryAcquire(SecurityIdentifier userSid, out HostSingleInstanceLease? lease)
    {
        string name = NameForUser(userSid);
        var mutex = new Mutex(initiallyOwned: true, name, out bool createdNew);
        if (!createdNew)
        {
            mutex.Dispose();
            lease = null;
            return false;
        }

        lease = new HostSingleInstanceLease(mutex);
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            _mutex.ReleaseMutex();
        }
        finally
        {
            _mutex.Dispose();
        }
    }
}
