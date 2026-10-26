namespace Converty.Core.Execution;

public sealed record ConversionFileResult(string InputPath, string OutputPath, int ExitCode);

public sealed record ConversionFileFailure(
    string InputPath,
    string? OutputPath,
    int? ExitCode,
    string Message);

public sealed record ConversionBatchResult(
    IReadOnlyList<ConversionFileResult> Files,
    IReadOnlyList<ConversionFileFailure> Failures)
{
    public ConversionBatchResult(IReadOnlyList<ConversionFileResult> files)
        : this(files, Array.Empty<ConversionFileFailure>())
    {
    }

    public bool HasFailures => Failures.Count > 0;
}
