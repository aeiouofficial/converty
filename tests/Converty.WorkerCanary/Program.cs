using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;

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
    File.WriteAllText(
        childPidPath,
        child.Id.ToString(CultureInfo.InvariantCulture));
    return 0;
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

if (args.Length == 2 && string.Equals(args[0], "--connect-loopback", StringComparison.Ordinal))
{
    int port = int.Parse(args[1], NumberStyles.None, CultureInfo.InvariantCulture);
    try
    {
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port).ConfigureAwait(false);
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
}

Console.Error.WriteLine("Unsupported Converty worker canary mode.");
return 64;
