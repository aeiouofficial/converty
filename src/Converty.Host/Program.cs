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

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            return 1;
        }

        string stateDirectory = Path.Combine(localAppData, "Converty", "state");
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
}
