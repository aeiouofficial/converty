using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;

namespace Converty.Bridge.Diagnostics;

[SupportedOSPlatform("windows")]
internal static class BridgePackageIdentityDiagnostic
{
    private const string ProbeMarker = "Converty.B2.BridgeIdentityProbe";
    private const int ErrorSuccess = 0;
    private const int ErrorInsufficientBuffer = 122;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static void TryWriteEvidence(IEnumerable<string> inputPaths)
    {
        ArgumentNullException.ThrowIfNull(inputPaths);

        string? probePath = inputPaths.FirstOrDefault(path =>
            Path.GetFileName(path).Contains(ProbeMarker, StringComparison.Ordinal));
        if (probePath is null)
        {
            return;
        }

        (int result, string? packageFamilyName) = ReadCurrentPackageFamilyName();
        string imagePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Unable to resolve the current Bridge executable image path.");

        var evidence = new
        {
            ProcessId = Environment.ProcessId,
            ImagePath = Path.GetFullPath(imagePath),
            PackageFamilyResult = result,
            PackageFamilyName = packageFamilyName,
        };

        string evidencePath = probePath + ".bridge-identity.json";
        File.WriteAllText(evidencePath, JsonSerializer.Serialize(evidence, JsonOptions));
    }

    private static (int Result, string? PackageFamilyName) ReadCurrentPackageFamilyName()
    {
        uint requiredLength = 0;
        int result = NativeMethods.GetCurrentPackageFamilyName(ref requiredLength, null);
        if (result != ErrorInsufficientBuffer || requiredLength <= 1)
        {
            return (result, null);
        }

        var packageFamilyName = new char[checked((int)requiredLength)];
        result = NativeMethods.GetCurrentPackageFamilyName(ref requiredLength, packageFamilyName);
        if (result != ErrorSuccess || requiredLength <= 1 || requiredLength > packageFamilyName.Length)
        {
            return (result, null);
        }

        int textLength = checked((int)requiredLength) - 1;
        if (textLength <= 0 || packageFamilyName[textLength] != '\0')
        {
            throw new InvalidOperationException("Current Bridge package family identity is empty or not terminated.");
        }

        return (ErrorSuccess, new string(packageFamilyName, 0, textLength));
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
