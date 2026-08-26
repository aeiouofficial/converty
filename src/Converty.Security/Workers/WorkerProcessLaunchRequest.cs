namespace Converty.Security.Workers;

public sealed record WorkerProcessLaunchRequest(
    string ExecutablePath,
    string WorkingDirectory,
    IReadOnlyList<string> Arguments,
    WorkerIsolationLevel IsolationLevel,
    WorkerResourceLimits ResourceLimits,
    TimeSpan Timeout,
    int MaximumCapturedStandardErrorCharacters);
