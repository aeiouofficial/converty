using System.Runtime.Versioning;
using Converty.Host.Runtime;

namespace Converty.Host;

internal static class Program
{
    private const int QueueCapacity = 256;

    [STAThread]
    [SupportedOSPlatform("windows")]
    private static async Task<int> Main()
    {
        if (!OperatingSystem.IsWindows())
        {
            return 1;
        }

        string? stateDirectory = ResolveStateDirectory();
        if (string.IsNullOrWhiteSpace(stateDirectory))
        {
            return 1;
        }

        string journalPath = Path.Combine(stateDirectory, "jobs-v1.json");
        var runtime = HostRuntime.CreateForCurrentUser(journalPath, QueueCapacity);

        using var shutdown = new CancellationTokenSource();
        void HandleProcessExit(object? sender, EventArgs args) => shutdown.Cancel();
        AppDomain.CurrentDomain.ProcessExit += HandleProcessExit;
        try
        {
            HostRuntimeResult result = await runtime.RunAsync(shutdown.Token);
            return result switch
            {
                HostRuntimeResult.Stopped => 0,
                HostRuntimeResult.AlreadyRunning => 0,
                _ => 1,
            };
        }
        finally
        {
            AppDomain.CurrentDomain.ProcessExit -= HandleProcessExit;
        }
    }

    private static string? ResolveStateDirectory()
    {
        string? workspaceRoot = Environment.GetEnvironmentVariable("CONVERTY_WORKSPACE_ROOT");
        if (!string.IsNullOrWhiteSpace(workspaceRoot))
        {
            if (!Path.IsPathFullyQualified(workspaceRoot))
            {
                return null;
            }

            try
            {
                workspaceRoot = Path.GetFullPath(workspaceRoot);
            }
            catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return null;
            }

            if (string.Equals(Path.GetPathRoot(workspaceRoot), @"C:\", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return Path.Combine(workspaceRoot, "_temp", "runtime", "Converty", "Host", "state");
        }

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return string.IsNullOrWhiteSpace(localAppData)
            ? null
            : Path.Combine(localAppData, "Converty", "state");
    }
}
