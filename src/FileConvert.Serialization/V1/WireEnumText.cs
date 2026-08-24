using System.Text.Json;
using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Jobs;

namespace FileConvert.Serialization.V1;

internal static class WireEnumText
{
    internal static string ToWire(ConversionAction value) => value switch
    {
        ConversionAction.ConvertUsingDefault => "convertUsingDefault",
        ConversionAction.ConvertToFormat => "convertToFormat",
        ConversionAction.ConvertWithPreset => "convertWithPreset",
        _ => throw new ArgumentOutOfRangeException(nameof(value), "Unsupported conversion action."),
    };

    internal static ConversionAction ParseConversionAction(string? value) => value switch
    {
        "convertUsingDefault" => ConversionAction.ConvertUsingDefault,
        "convertToFormat" => ConversionAction.ConvertToFormat,
        "convertWithPreset" => ConversionAction.ConvertWithPreset,
        _ => throw new JsonException("Invalid conversion action wire value."),
    };

    internal static string ToWire(ConversionMode value) => value switch
    {
        ConversionMode.Copy => "copy",
        ConversionMode.Remux => "remux",
        ConversionMode.Transcode => "transcode",
        ConversionMode.Transform => "transform",
        _ => throw new ArgumentOutOfRangeException(nameof(value), "Unsupported conversion mode."),
    };

    internal static ConversionMode ParseConversionMode(string? value) => value switch
    {
        "copy" => ConversionMode.Copy,
        "remux" => ConversionMode.Remux,
        "transcode" => ConversionMode.Transcode,
        "transform" => ConversionMode.Transform,
        _ => throw new JsonException("Invalid conversion mode wire value."),
    };

    internal static string ToWire(ConversionJobState value) => value switch
    {
        ConversionJobState.Queued => "queued",
        ConversionJobState.Probing => "probing",
        ConversionJobState.Planning => "planning",
        ConversionJobState.Staging => "staging",
        ConversionJobState.Converting => "converting",
        ConversionJobState.Validating => "validating",
        ConversionJobState.Committing => "committing",
        ConversionJobState.Completed => "completed",
        ConversionJobState.Failed => "failed",
        ConversionJobState.Cancelled => "cancelled",
        ConversionJobState.Rejected => "rejected",
        _ => throw new ArgumentOutOfRangeException(nameof(value), "Unsupported conversion job state."),
    };

    internal static ConversionJobState ParseConversionJobState(string? value) => value switch
    {
        "queued" => ConversionJobState.Queued,
        "probing" => ConversionJobState.Probing,
        "planning" => ConversionJobState.Planning,
        "staging" => ConversionJobState.Staging,
        "converting" => ConversionJobState.Converting,
        "validating" => ConversionJobState.Validating,
        "committing" => ConversionJobState.Committing,
        "completed" => ConversionJobState.Completed,
        "failed" => ConversionJobState.Failed,
        "cancelled" => ConversionJobState.Cancelled,
        "rejected" => ConversionJobState.Rejected,
        _ => throw new JsonException("Invalid conversion job state wire value."),
    };
}
