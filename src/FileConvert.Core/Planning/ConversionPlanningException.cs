namespace FileConvert.Core.Planning;

public sealed class ConversionPlanningException : InvalidOperationException
{
    public ConversionPlanningException(string message)
        : base(message)
    {
    }
}
