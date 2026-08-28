using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Converty.PackageIdentityCanary;

internal static class Program
{
    private const int ErrorInsufficientBuffer = 122;
    private const int AppModelErrorNoPackage = 15700;
    private static readonly JsonSerializerOptions EvidenceJsonOptions = new() { WriteIndented = true };

    public static int Main(string[] args)
    {
        if (!OperatingSystem.IsWindows())
        {
            return 2;
        }

        if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            return 3;
        }

        string evidencePath = Path.GetFullPath(args[0]);
        string hostPath = Path.Combine(AppContext.BaseDirectory, "Converty.Host.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);

        string? parentFamily = ReadCurrentPackageFamilyName(out int parentError);
        int childError = 0;
        string? childFamily = null;
        int childPid = 0;
        string? failure = null;

        Process? child = null;
        try
        {
            if (!File.Exists(hostPath))
            {
                failure = $"Host executable is missing: {hostPath}";
            }
            else
            {
                child = Process.Start(new ProcessStartInfo
                {
                    FileName = hostPath,
                    WorkingDirectory = AppContext.BaseDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });

                if (child is null)
                {
                    failure = "Process.Start returned null for Converty.Host.exe.";
                }
                else
                {
                    childPid = child.Id;
                    childFamily = ReadProcessPackageFamilyName(child.Handle, out childError);
                }
            }
        }
        catch (Exception ex)
        {
            failure = ex.ToString();
        }
        finally
        {
            if (child is not null)
            {
                try
                {
                    if (!child.HasExited)
                    {
                        child.Kill(entireProcessTree: true);
                        child.WaitForExit(5000);
                    }
                }
                catch
                {
                    // Diagnostic cleanup must not hide the recorded identity result.
                }
                finally
                {
                    child.Dispose();
                }
            }
        }

        var evidence = new
        {
            ParentProcessId = Environment.ProcessId,
            ParentPackageFamilyName = parentFamily,
            ParentPackageFamilyError = parentError,
            ChildProcessId = childPid,
            ChildPackageFamilyName = childFamily,
            ChildPackageFamilyError = childError,
            HostPath = hostPath,
            Failure = failure,
        };

        File.WriteAllText(
            evidencePath,
            JsonSerializer.Serialize(evidence, EvidenceJsonOptions) + Environment.NewLine,
            Encoding.UTF8);

        return failure is null ? 0 : 4;
    }

    private static string? ReadCurrentPackageFamilyName(out int error)
    {
        uint length = 0;
        error = GetCurrentPackageFamilyName(ref length, null);
        if (error == AppModelErrorNoPackage)
        {
            return null;
        }
        if (error != ErrorInsufficientBuffer)
        {
            return null;
        }

        var buffer = new StringBuilder(checked((int)length));
        error = GetCurrentPackageFamilyName(ref length, buffer);
        return error == 0 ? buffer.ToString() : null;
    }

    private static string? ReadProcessPackageFamilyName(IntPtr processHandle, out int error)
    {
        uint length = 0;
        error = GetPackageFamilyName(processHandle, ref length, null);
        if (error == AppModelErrorNoPackage)
        {
            return null;
        }
        if (error != ErrorInsufficientBuffer)
        {
            return null;
        }

        var buffer = new StringBuilder(checked((int)length));
        error = GetPackageFamilyName(processHandle, ref length, buffer);
        return error == 0 ? buffer.ToString() : null;
    }

#pragma warning disable CA1838 // Temporary diagnostic P/Invoke mirrors the production package-family probe shape.
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetCurrentPackageFamilyName(
        ref uint packageFamilyNameLength,
        [Out] StringBuilder? packageFamilyName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetPackageFamilyName(
        IntPtr process,
        ref uint packageFamilyNameLength,
        [Out] StringBuilder? packageFamilyName);
#pragma warning restore CA1838
}
