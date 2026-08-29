using System.ComponentModel;
using System.Threading;

namespace Converty.Security.Workers;

internal sealed class WindowsAclGrant : IDisposable
{
    private const string AclMutationMutexName = @"Local\Converty.StrictWorkerAclMutation.v1";
    private static readonly TimeSpan AclMutationMutexTimeout = TimeSpan.FromSeconds(30);
    private static readonly Mutex AclMutationMutex = new(initiallyOwned: false, AclMutationMutexName);

    private readonly nint _sid;
    private readonly List<string> _grantedPaths;
    private bool _disposed;

    private WindowsAclGrant(nint sid, List<string> grantedPaths)
    {
        _sid = sid;
        _grantedPaths = grantedPaths;
    }

    internal static WindowsAclGrant GrantApplicationReadExecute(string root, nint sid) =>
        GrantTree(
            root,
            sid,
            WindowsNativeMethods.FILE_GENERIC_READ | WindowsNativeMethods.FILE_GENERIC_EXECUTE);

    internal static WindowsAclGrant GrantStagingReadWrite(string root, nint sid) =>
        GrantTree(
            root,
            sid,
            WindowsNativeMethods.FILE_GENERIC_READ | WindowsNativeMethods.FILE_GENERIC_WRITE);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        List<Exception>? failures = null;
        for (int index = _grantedPaths.Count - 1; index >= 0; index--)
        {
            try
            {
                UpdatePathAcl(
                    _grantedPaths[index],
                    _sid,
                    accessPermissions: 0,
                    WindowsNativeMethods.REVOKE_ACCESS);
            }
            catch (Exception exception) when (exception is Win32Exception or IOException)
            {
                failures ??= [];
                failures.Add(exception);
            }
        }

        if (failures is not null)
        {
            throw new AggregateException("Converty could not remove one or more strict worker ACL grants.", failures);
        }
    }

    private static WindowsAclGrant GrantTree(string root, nint sid, uint accessPermissions)
    {
        if (sid == nint.Zero)
        {
            throw new ArgumentException("AppContainer SID is required.", nameof(sid));
        }

        string fullRoot = Path.GetFullPath(root);
        if (!Directory.Exists(fullRoot))
        {
            throw new DirectoryNotFoundException("Strict worker ACL root does not exist.");
        }

        List<string> paths = [fullRoot];
        foreach (string path in Directory.EnumerateFileSystemEntries(fullRoot, "*", SearchOption.AllDirectories))
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException("Strict worker ACL scope must not cross a reparse point.");
            }
            paths.Add(path);
        }

        var granted = new List<string>(paths.Count);
        try
        {
            foreach (string path in paths)
            {
                UpdatePathAcl(path, sid, accessPermissions, WindowsNativeMethods.GRANT_ACCESS);
                granted.Add(path);
            }
            return new WindowsAclGrant(sid, granted);
        }
        catch
        {
            var partialGrant = new WindowsAclGrant(sid, granted);
            partialGrant.Dispose();
            throw;
        }
    }

    private static void UpdatePathAcl(string path, nint sid, uint accessPermissions, uint accessMode)
    {
        bool ownsMutationMutex = false;
        try
        {
            try
            {
                ownsMutationMutex = AclMutationMutex.WaitOne(AclMutationMutexTimeout);
            }
            catch (AbandonedMutexException)
            {
                // Windows transfers ownership when a prior Converty process exits while
                // mutating an ACL, so continue from the freshly re-read security state.
                ownsMutationMutex = true;
            }

            if (!ownsMutationMutex)
            {
                throw new TimeoutException("Converty timed out waiting to update a strict worker ACL.");
            }

            uint result = WindowsNativeMethods.GetNamedSecurityInfoW(
                path,
                WindowsNativeMethods.SE_FILE_OBJECT,
                WindowsNativeMethods.DACL_SECURITY_INFORMATION,
                out _,
                out _,
                out nint oldAcl,
                out _,
                out nint securityDescriptor);
            if (result != 0)
            {
                throw new Win32Exception(checked((int)result), "Converty could not read a filesystem ACL for strict worker isolation.");
            }

            nint newAcl = nint.Zero;
            try
            {
                bool directory = Directory.Exists(path);
                var entry = new WindowsNativeMethods.ExplicitAccess
                {
                    AccessPermissions = accessPermissions,
                    AccessMode = accessMode,
                    Inheritance = directory
                        ? WindowsNativeMethods.OBJECT_INHERIT_ACE | WindowsNativeMethods.CONTAINER_INHERIT_ACE
                        : 0,
                    Trustee = new WindowsNativeMethods.Trustee
                    {
                        MultipleTrustee = nint.Zero,
                        MultipleTrusteeOperation = 0,
                        TrusteeForm = checked((int)WindowsNativeMethods.TRUSTEE_IS_SID),
                        TrusteeType = checked((int)WindowsNativeMethods.TRUSTEE_IS_UNKNOWN),
                        Name = sid,
                    },
                };

                result = WindowsNativeMethods.SetEntriesInAclW(1, ref entry, oldAcl, out newAcl);
                if (result != 0)
                {
                    throw new Win32Exception(checked((int)result), "Converty could not construct a scoped strict worker ACL.");
                }

                result = WindowsNativeMethods.SetNamedSecurityInfoW(
                    path,
                    WindowsNativeMethods.SE_FILE_OBJECT,
                    WindowsNativeMethods.DACL_SECURITY_INFORMATION,
                    nint.Zero,
                    nint.Zero,
                    newAcl,
                    nint.Zero);
                if (result != 0)
                {
                    throw new Win32Exception(checked((int)result), "Converty could not apply a scoped strict worker ACL.");
                }
            }
            finally
            {
                if (newAcl != nint.Zero)
                {
                    _ = WindowsNativeMethods.LocalFree(newAcl);
                }
                if (securityDescriptor != nint.Zero)
                {
                    _ = WindowsNativeMethods.LocalFree(securityDescriptor);
                }
            }
        }
        finally
        {
            if (ownsMutationMutex)
            {
                AclMutationMutex.ReleaseMutex();
            }
        }
    }
}
