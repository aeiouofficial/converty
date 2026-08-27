using System.Runtime.InteropServices;

namespace Converty.Security.Workers;

internal sealed class WindowsAppContainerProfile : IDisposable
{
    private readonly string _profileName;
    private readonly bool _ownsProfile;
    private nint _sid;

    private WindowsAppContainerProfile(string profileName, nint sid, bool ownsProfile)
    {
        _profileName = profileName;
        _sid = sid;
        _ownsProfile = ownsProfile;
    }

    internal nint Sid =>
        _sid != nint.Zero
            ? _sid
            : throw new ObjectDisposedException(nameof(WindowsAppContainerProfile));

    internal static WindowsAppContainerProfile Create()
    {
        string profileName = $"Converty.Worker.{Guid.NewGuid():N}";
        int result = WindowsNativeMethods.CreateAppContainerProfile(
            profileName,
            "Converty worker",
            "Disposable Converty media conversion worker",
            nint.Zero,
            capabilityCount: 0,
            out nint sid);

        bool ownsProfile = true;
        if (result == WindowsNativeMethods.ERROR_ALREADY_EXISTS_HRESULT)
        {
            result = WindowsNativeMethods.DeriveAppContainerSidFromAppContainerName(profileName, out sid);
            ownsProfile = false;
        }
        if (result < 0)
        {
            Marshal.ThrowExceptionForHR(result);
        }
        if (sid == nint.Zero)
        {
            if (ownsProfile)
            {
                _ = WindowsNativeMethods.DeleteAppContainerProfile(profileName);
            }
            throw new InvalidOperationException("Windows did not return an AppContainer SID for the strict worker.");
        }

        return new WindowsAppContainerProfile(profileName, sid, ownsProfile);
    }

    public void Dispose()
    {
        if (_sid != nint.Zero)
        {
            _ = WindowsNativeMethods.FreeSid(_sid);
            _sid = nint.Zero;
        }

        if (_ownsProfile)
        {
            int result = WindowsNativeMethods.DeleteAppContainerProfile(_profileName);
            if (result < 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }
        }
    }
}
