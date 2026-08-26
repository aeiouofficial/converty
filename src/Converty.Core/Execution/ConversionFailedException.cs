namespace Converty.Core.Execution;

public sealed class ConversionFailedException : IOException
{
    public ConversionFailedException(
        string inputPath,
        string outputPath,
        int? exitCode,
        string message)
        : base(message)
    {
        InputPath = inputPath;
        OutputPath = outputPath;
        ExitCode = exitCode;
    }

    public string InputPath { get; }
    public string OutputPath { get; }
    public int? ExitCode { get; }
}
