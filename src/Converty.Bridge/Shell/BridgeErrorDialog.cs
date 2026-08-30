using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Converty.Bridge.Shell;

internal static class BridgeErrorDialog
{
    internal const string NonInteractiveEnvironmentVariable = "CONVERTY_BRIDGE_NONINTERACTIVE";
    private const uint MbOk = 0x00000000;
    private const uint MbIconError = 0x00000010;
    private const int MaximumMessageCharacters = 2048;

    [SupportedOSPlatform("windows")]
    public static void Show(string message)
    {
        string boundedMessage = string.IsNullOrWhiteSpace(message)
            ? "Converty could not complete the conversion."
            : message.Length <= MaximumMessageCharacters
                ? message
                : message[..MaximumMessageCharacters];

        // Automation must observe the same bounded Bridge failure path without blocking
        // on desktop UI. Explorer does not set this explicit opt-in and keeps the dialog.
        if (string.Equals(
                Environment.GetEnvironmentVariable(NonInteractiveEnvironmentVariable),
                "1",
                StringComparison.Ordinal))
        {
            Console.Error.WriteLine(boundedMessage);
            return;
        }

        _ = MessageBoxW(IntPtr.Zero, boundedMessage, "Converty", MbOk | MbIconError);
    }

#pragma warning disable SYSLIB1054 // Small fixed Win32 UI boundary; no user-controlled library or entry-point selection.
    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int MessageBoxW(IntPtr window, string text, string caption, uint type);
#pragma warning restore SYSLIB1054
}