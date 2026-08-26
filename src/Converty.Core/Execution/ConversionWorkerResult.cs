namespace Converty.Core.Execution;

public sealed record ConversionWorkerResult(int ExitCode, string StandardError)
{
    public bool Succeeded => ExitCode == 0;
}
