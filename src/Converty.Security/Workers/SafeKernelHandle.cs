using Microsoft.Win32.SafeHandles;

namespace Converty.Security.Workers;

internal sealed class SafeKernelHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    internal SafeKernelHandle(nint handle, bool ownsHandle)
        : base(ownsHandle)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle() => WindowsNativeMethods.CloseHandle(handle);
}
