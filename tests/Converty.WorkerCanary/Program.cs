using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;

if (args.Length == 1 && string.Equals(args[0], "--hold", StringComparison.Ordinal))
{
    await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false);
    return 0;
}

if (args.Length == 2 && string.Equals(args[0], "--spawn-child-and-exit", StringComparison.Ordinal))
{
    string executablePath = Environment.ProcessPath ??
        throw new InvalidOperationException("Canary process path is unavailable.");
    string childPidPath = Path.GetFullPath(args[1]);

    var startInfo = new ProcessStartInfo
    {
        FileName = executablePath,
        UseShellExecute = false,
        CreateNoWindow = true,
    };
    startInfo.ArgumentList.Add("--hold");

    using Process child = Process.Start(startInfo) ??
        throw new InvalidOperationException("Canary child process could not be started.");
    File.WriteAllText(childPidPath, child.Id.ToString(CultureInfo.InvariantCulture));
    return 0;
}

if (args.Length == 2 && string.Equals(args[0], "--read-file", StringComparison.Ordinal))
{
    try
    {
        _ = File.ReadAllBytes(Path.GetFullPath(args[1]));
        return 0;
    }
    catch (UnauthorizedAccessException)
    {
        return 13;
    }
    catch (IOException)
    {
        return 13;
    }
}

if (args.Length == 2 && string.Equals(args[0], "--write-file", StringComparison.Ordinal))
{
    try
    {
        File.WriteAllText(Path.GetFullPath(args[1]), "Converty strict isolation canary");
        return 0;
    }
    catch (UnauthorizedAccessException)
    {
        return 13;
    }
    catch (IOException)
    {
        return 13;
    }
}

if (args.Length == 2 && string.Equals(args[0], "--stdout-bytes", StringComparison.Ordinal))
{
    long requestedBytes = long.Parse(args[1], NumberStyles.None, CultureInfo.InvariantCulture);
    await WriteStdoutBytesAsync(requestedBytes).ConfigureAwait(false);
    return 0;
}

if (args.Length == 3 && string.Equals(args[0], "--stdout-bytes-spawn-child-and-hold", StringComparison.Ordinal))
{
    long requestedBytes = long.Parse(args[1], NumberStyles.None, CultureInfo.InvariantCulture);
    string childPidPath = Path.GetFullPath(args[2]);
    string executablePath = Environment.ProcessPath ??
        throw new InvalidOperationException("Canary process path is unavailable.");

    var startInfo = new ProcessStartInfo
    {
        FileName = executablePath,
        UseShellExecute = false,
        CreateNoWindow = true,
    };
    startInfo.ArgumentList.Add("--hold");

    using Process child = Process.Start(startInfo) ??
        throw new InvalidOperationException("Canary child process could not be started.");
    File.WriteAllText(childPidPath, child.Id.ToString(CultureInfo.InvariantCulture));
    await WriteStdoutBytesAsync(requestedBytes).ConfigureAwait(false);
    await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false);
    return 0;
}

if (args.Length == 3 &&
    (string.Equals(args[0], "--write-slow-bytes", StringComparison.Ordinal) ||
     string.Equals(args[0], "--write-slow-bytes-and-hold", StringComparison.Ordinal)))
{
    string outputPath = Path.GetFullPath(args[1]);
    long requestedBytes = long.Parse(args[2], NumberStyles.None, CultureInfo.InvariantCulture);
    byte[] buffer = new byte[4096];
    await using var output = new FileStream(
        outputPath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.Read,
        bufferSize: buffer.Length,
        useAsync: true);
    long written = 0;
    while (written < requestedBytes)
    {
        int count = checked((int)Math.Min(buffer.Length, requestedBytes - written));
        await output.WriteAsync(buffer.AsMemory(0, count)).ConfigureAwait(false);
        await output.FlushAsync().ConfigureAwait(false);
        written += count;
        await Task.Delay(TimeSpan.FromMilliseconds(5)).ConfigureAwait(false);
    }

    if (string.Equals(args[0], "--write-slow-bytes-and-hold", StringComparison.Ordinal))
    {
        await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false);
    }

    return 0;
}

if (args.Length == 2 && string.Equals(args[0], "--connect-loopback", StringComparison.Ordinal))
{
    int port = int.Parse(args[1], NumberStyles.None, CultureInfo.InvariantCulture);
    try
    {
        using var client = new TcpClient();
        using var connectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await client.ConnectAsync("127.0.0.1", port, connectTimeout.Token).ConfigureAwait(false);
        return 0;
    }
    catch (SocketException)
    {
        return 13;
    }
    catch (UnauthorizedAccessException)
    {
        return 13;
    }
    catch (OperationCanceledException)
    {
        return 13;
    }
}

if (args.Length == 3 && string.Equals(args[0], "--spawn-ffprobe-read", StringComparison.Ordinal))
{
    string ffprobe = Path.GetFullPath(args[1]);
    string input = Path.GetFullPath(args[2]);
    return await RunEngineAsync(
        CreateEngineStartInfo(
            ffprobe,
            [
                "-v", "error",
                "-show_format",
                "-show_streams",
                "-of", "json",
                "-protocol_whitelist", "file",
                "-format_whitelist", "mov,matroska,avi,mpeg,asf,mp3",
                input,
            ])).ConfigureAwait(false);
}

if (args.Length == 3 && string.Equals(args[0], "--spawn-ffmpeg-write-wave", StringComparison.Ordinal))
{
    string ffmpeg = Path.GetFullPath(args[1]);
    string output = Path.GetFullPath(args[2]);
    return await RunEngineAsync(
        CreateEngineStartInfo(
            ffmpeg,
            [
                "-hide_banner", "-loglevel", "error", "-nostdin", "-y",
                "-f", "lavfi", "-i", "sine=frequency=1000:sample_rate=8000",
                "-t", "0.10",
                "-c:a", "pcm_s16le",
                "-f", "wav",
                output,
            ])).ConfigureAwait(false);
}

if (args.Length == 3 && string.Equals(args[0], "--spawn-ffmpeg-hold", StringComparison.Ordinal))
{
    string ffmpeg = Path.GetFullPath(args[1]);
    string pidPath = Path.GetFullPath(args[2]);
    return await RunEngineAsync(
        CreateEngineStartInfo(
            ffmpeg,
            [
                "-hide_banner", "-loglevel", "error", "-nostdin",
                "-re",
                "-f", "lavfi", "-i", "testsrc2=size=64x48:rate=10",
                "-t", "120",
                "-f", "null", "-",
            ]),
        pidPath).ConfigureAwait(false);
}

if (args.Length == 3 && string.Equals(args[0], "--spawn-ffmpeg-connect-loopback", StringComparison.Ordinal))
{
    string ffmpeg = Path.GetFullPath(args[1]);
    int port = int.Parse(args[2], NumberStyles.None, CultureInfo.InvariantCulture);
    return await RunEngineAsync(
        CreateEngineStartInfo(
            ffmpeg,
            [
                "-hide_banner", "-loglevel", "error", "-nostdin",
                "-f", "lavfi", "-i", "testsrc2=size=32x24:rate=1",
                "-t", "1",
                "-c:v", "mpeg2video",
                "-f", "mpegts",
                $"tcp://127.0.0.1:{port}",
            ])).ConfigureAwait(false);
}

if (args.Length == 3 && string.Equals(args[0], "--spawn-ffprobe-connect-loopback", StringComparison.Ordinal))
{
    string ffprobe = Path.GetFullPath(args[1]);
    int port = int.Parse(args[2], NumberStyles.None, CultureInfo.InvariantCulture);
    return await RunEngineAsync(
        CreateEngineStartInfo(
            ffprobe,
            [
                "-v", "error",
                "-show_format",
                "-of", "json",
                "-protocol_whitelist", "tcp",
                $"tcp://127.0.0.1:{port}",
            ])).ConfigureAwait(false);
}

Console.Error.WriteLine("Unsupported Converty worker canary mode.");
return 64;

static ProcessStartInfo CreateEngineStartInfo(string executable, IReadOnlyList<string> arguments)
{
    string workingDirectory = Path.GetDirectoryName(executable) ??
        throw new InvalidOperationException("Engine executable requires a parent directory.");
    var startInfo = new ProcessStartInfo
    {
        FileName = executable,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        WorkingDirectory = workingDirectory,
    };
    foreach (string argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }
    return startInfo;
}

static async Task<int> RunEngineAsync(ProcessStartInfo startInfo, string? pidPath = null)
{
    try
    {
        using var child = new Process { StartInfo = startInfo };
        if (!child.Start())
        {
            return 13;
        }

        if (!string.IsNullOrWhiteSpace(pidPath))
        {
            File.WriteAllText(pidPath, child.Id.ToString(CultureInfo.InvariantCulture));
        }

        Console.WriteLine($"child_appcontainer={(IsAppContainerProcess(child) ? 1 : 0)}");
        Task<string> stdout = child.StandardOutput.ReadToEndAsync();
        Task<string> stderr = child.StandardError.ReadToEndAsync();
        await child.WaitForExitAsync().ConfigureAwait(false);
        _ = await stdout.ConfigureAwait(false);
        string error = await stderr.ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.Error.Write(error);
        }
        return child.ExitCode;
    }
    catch (Exception error) when (error is UnauthorizedAccessException or IOException or Win32Exception or InvalidOperationException)
    {
        Console.Error.WriteLine(error.Message);
        return 13;
    }
}

static bool IsAppContainerProcess(Process process)
{
    const uint tokenQuery = 0x0008;
    const int tokenIsAppContainer = 29;
    if (!NativeMethods.OpenProcessToken(process.Handle, tokenQuery, out nint token))
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not open child process token.");
    }

    try
    {
        int isAppContainer = 0;
        if (!NativeMethods.GetTokenInformation(token, tokenIsAppContainer, out isAppContainer, sizeof(int), out _))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not inspect child AppContainer token state.");
        }
        return isAppContainer != 0;
    }
    finally
    {
        _ = NativeMethods.CloseHandle(token);
    }
}

static async Task WriteStdoutBytesAsync(long requestedBytes)
{
    byte[] buffer = new byte[4096];
    Array.Fill(buffer, (byte)'x');
    await using Stream output = Console.OpenStandardOutput();
    long written = 0;
    while (written < requestedBytes)
    {
        int count = checked((int)Math.Min(buffer.Length, requestedBytes - written));
        await output.WriteAsync(buffer.AsMemory(0, count)).ConfigureAwait(false);
        await output.FlushAsync().ConfigureAwait(false);
        written += count;
    }
}

internal static class NativeMethods
{
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool OpenProcessToken(nint processHandle, uint desiredAccess, out nint tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetTokenInformation(
        nint tokenHandle,
        int tokenInformationClass,
        out int tokenInformation,
        int tokenInformationLength,
        out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CloseHandle(nint handle);
}
