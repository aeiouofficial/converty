using Converty.Contracts.Conversion;

namespace Converty.Core.Execution;

public static class ConversionModeArgument
{
    public static string Format(ConversionMode mode) => mode switch
    {
        ConversionMode.Copy => "copy",
        ConversionMode.Remux => "remux",
        ConversionMode.Transcode => "transcode",
        ConversionMode.Transform => "transform",
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
    };

    public static ConversionMode Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Conversion mode is required.", nameof(value));
        }

        return value switch
        {
            "copy" => ConversionMode.Copy,
            "remux" => ConversionMode.Remux,
            "transcode" => ConversionMode.Transcode,
            "transform" => ConversionMode.Transform,
            _ => throw new ArgumentException("Unsupported conversion mode.", nameof(value)),
        };
    }
}
