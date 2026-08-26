namespace Converty.Core.Execution;

public sealed record ConversionFileResult(string InputPath, string OutputPath, int ExitCode);

public sealed record ConversionBatchResult(IReadOnlyList<ConversionFileResult> Files);
