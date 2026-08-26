using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Converty.Bridge.Ipc;

[SupportedOSPlatform("windows")]
public static class WindowsCurrentPackageFamilyName
{
    private const int ErrorSuccess = 0;
    private const int ErrorInsufficientBuffer = 122;
    private const int AppModelErrorNoPackage = 15700;

    public static string GetRequired()
    {
        uint requiredLength = 0;
        int result = NativeMethods.GetCurrentPackageFamilyName(ref requiredLength, null);
        if (result == AppModelErrorNoPackage)
        {
            throw new InvalidOperationException(
                "Current Converty Bridge process has no Windows package family identity.");
        }

        if (result != ErrorInsufficientBuffer || requiredLength <= 1)
        {
            throw new InvalidOperationException(
                $"Unable to determine the current Converty package family identity (Win32 error {result}).");
        }

        var packageFamilyName = new char[checked((int)requiredLength)];
        result = NativeMethods.GetCurrentPackageFamilyName(
            ref requiredLength,
            packageFamilyName);
        if (result != ErrorSuccess || requiredLength <= 1 || requiredLength > packageFamilyName.Length)
        {
            throw new InvalidOperationException(
                $"Unable to read the current Converty package family identity (Win32 error {result}).");
        }

        int textLength = checked((int)requiredLength) - 1;
        if (textLength <= 0 || packageFamilyName[textLength] != '\0')
        {
            throw new InvalidOperationException(
                "Current Converty package family identity is empty or not terminated.");
        }

        return new string(packageFamilyName, 0, textLength);
    }

    private static class NativeMethods
    {
        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode,
            ExactSpelling = true)]
        internal static extern int GetCurrentPackageFamilyName(
            ref uint packageFamilyNameLength,
            [Out] char[]? packageFamilyName);
    }
}
