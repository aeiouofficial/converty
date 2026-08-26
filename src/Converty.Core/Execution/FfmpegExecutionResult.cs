namespace Converty.Core.Execution;

public sealed record FfmpegExecutionResult(int ExitCode, string StandardError)
{
    public bool Succeeded => ExitCode == 0;
}
