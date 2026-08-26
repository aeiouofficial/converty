namespace Converty.Security.Workers;

public sealed record WorkerProcessLaunchRequest(
    string ExecutablePath,
    string WorkingDirectory,
    IReadOnlyList<string> Arguments,
    TimeSpan Timeout,
    int MaximumCapturedStandardErrorCharacters);
