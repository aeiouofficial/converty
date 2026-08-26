using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace Converty.Bridge.Ipc;

[SupportedOSPlatform("windows")]
public sealed class WindowsConnectedServerIdentityProbe : IConnectedServerIdentityProbe
{
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int ErrorSuccess = 0;
    private const int ErrorInsufficientBuffer = 122;
    private const int AppModelErrorNoPackage = 15700;
    private const int MaximumImagePathCharacters = 32_768;

    public ConnectedServerIdentitySnapshot Capture(NamedPipeClientStream pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);

        if (!pipe.IsConnected)
        {
            throw new BridgeServerIdentityException(
                "Connected server identity requires an active named-pipe session.");
        }

        uint serverProcessId = ReadServerProcessId(pipe);

        using SafeProcessHandle process = NativeMethods.OpenProcess(
            ProcessQueryLimitedInformation,
            false,
            serverProcessId);
        if (process.IsInvalid)
        {
            throw NativeFailure(
                "Unable to open the connected server process for identity verification.");
        }

        string imagePath = ReadImagePath(process);
        string packageFamilyName = ReadPackageFamilyName(process);
        uint confirmedServerProcessId = ReadServerProcessId(pipe);

        return new ConnectedServerIdentitySnapshot(
            serverProcessId,
            imagePath,
            packageFamilyName,
            confirmedServerProcessId);
    }

    private static uint ReadServerProcessId(NamedPipeClientStream pipe)
    {
        if (!NativeMethods.GetNamedPipeServerProcessId(pipe.SafePipeHandle, out uint processId)
            || processId == 0)
        {
            throw NativeFailure(
                "Unable to determine the connected named-pipe server process identity.");
        }

        return processId;
    }

    private static string ReadImagePath(SafeProcessHandle process)
    {
        var imagePath = new char[MaximumImagePathCharacters];
        uint length = MaximumImagePathCharacters;
        if (!NativeMethods.QueryFullProcessImageNameW(process, 0, imagePath, ref length)
            || length == 0
            || length > imagePath.Length)
        {
            throw NativeFailure(
                "Unable to determine the connected server executable image path.");
        }

        return new string(imagePath, 0, checked((int)length));
    }

    private static string ReadPackageFamilyName(SafeProcessHandle process)
    {
        uint requiredLength = 0;
        int result = NativeMethods.GetPackageFamilyName(process, ref requiredLength, null);
        if (result == AppModelErrorNoPackage)
        {
            throw new BridgeServerIdentityException(
                "Connected server process has no Windows package family identity.");
        }

        if (result != ErrorInsufficientBuffer || requiredLength <= 1)
        {
            throw new BridgeServerIdentityException(
                $"Unable to determine the connected server package family identity (Win32 error {result}).");
        }

        var packageFamilyName = new char[checked((int)requiredLength)];
        result = NativeMethods.GetPackageFamilyName(
            process,
            ref requiredLength,
            packageFamilyName);
        if (result != ErrorSuccess || requiredLength <= 1 || requiredLength > packageFamilyName.Length)
        {
            throw new BridgeServerIdentityException(
                $"Unable to read the connected server package family identity (Win32 error {result}).");
        }

        int textLength = checked((int)requiredLength) - 1;
        if (textLength <= 0 || packageFamilyName[textLength] != '\0')
        {
            throw new BridgeServerIdentityException(
                "Connected server package family identity is empty or not terminated.");
        }

        return new string(packageFamilyName, 0, textLength);
    }

    private static BridgeServerIdentityException NativeFailure(string message)
    {
        return new BridgeServerIdentityException(
            message,
            new Win32Exception(Marshal.GetLastPInvokeError()));
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetNamedPipeServerProcessId(
            SafePipeHandle pipe,
            out uint serverProcessId);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern SafeProcessHandle OpenProcess(
            uint desiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
            uint processId);

        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool QueryFullProcessImageNameW(
            SafeProcessHandle process,
            uint flags,
            [Out] char[] imagePath,
            ref uint size);

        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode,
            ExactSpelling = true)]
        internal static extern int GetPackageFamilyName(
            SafeProcessHandle process,
            ref uint packageFamilyNameLength,
            [Out] char[]? packageFamilyName);
    }
}
