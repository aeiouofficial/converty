using System.Text.Json;
using Converty.Contracts.Conversion;
using Converty.Serialization.V1;

namespace Converty.Serialization;

public static partial class ContractJson
{
    public static string Serialize(MediaProbeResultV1 value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(ToWire(value), SerializerOptions);
    }

    public static MediaProbeResultV1 DeserializeMediaProbeResult(string json) =>
        Dispatch(json, "media probe result", DeserializeMediaProbeResultV1);

    private static MediaProbeResultV1 DeserializeMediaProbeResultV1(string json)
    {
        MediaProbeResultWire wire = DeserializeWire<MediaProbeResultWire>(json, "media probe result");
        MediaProbeStatus status = ProbeWireEnumText.ParseStatus(wire.Status);
        MediaProbeFailureReason failureReason = ProbeWireEnumText.ParseFailureReason(wire.FailureReason);

        return status switch
        {
            MediaProbeStatus.Success when wire.Facts is not null && failureReason == MediaProbeFailureReason.None =>
                MediaProbeResultV1.Success(FromWire(wire.Facts)),
            MediaProbeStatus.Failure when wire.Facts is null && failureReason != MediaProbeFailureReason.None =>
                MediaProbeResultV1.Failure(failureReason),
            MediaProbeStatus.Success => throw new JsonException("Successful media probe result requires facts and failureReason none."),
            MediaProbeStatus.Failure => throw new JsonException("Failed media probe result requires no facts and a non-none failureReason."),
            _ => throw new JsonException("Invalid media probe result status."),
        };
    }

    private static MediaProbeResultWire ToWire(MediaProbeResultV1 value) => new()
    {
        SchemaVersion = MediaProbeResultV1.SchemaVersion,
        Status = ProbeWireEnumText.ToWire(value.Status),
        Facts = value.Facts is null ? null : ToWire(value.Facts),
        FailureReason = ProbeWireEnumText.ToWire(value.FailureReason),
    };

    private static MediaProbeFactsWire ToWire(MediaProbeFactsV1 value) => new()
    {
        Container = ProbeWireEnumText.ToWire(value.Container),
        Streams = value.Streams.Select(ToWire).ToArray(),
        Completeness = ProbeWireEnumText.ToWire(value.Completeness),
        HasChapters = value.HasChapters,
        HasGlobalMetadata = value.HasGlobalMetadata,
        HasPolicyRelevantStreamMetadata = value.HasPolicyRelevantStreamMetadata,
    };

    private static MediaStreamFactsWire ToWire(MediaStreamFactsV1 value) => new()
    {
        Index = value.Index,
        Kind = ProbeWireEnumText.ToWire(value.Kind),
        Codec = ProbeWireEnumText.ToWire(value.Codec),
        Profile = ProbeWireEnumText.ToWire(value.Profile),
        IsDefault = value.IsDefault,
        IsAttachedPicture = value.IsAttachedPicture,
        PixelFormat = ProbeWireEnumText.ToWire(value.PixelFormat),
        BitDepth = value.BitDepth,
        Width = value.Width,
        Height = value.Height,
        ColorTransfer = ProbeWireEnumText.ToWire(value.ColorTransfer),
        HdrState = ProbeWireEnumText.ToWire(value.HdrState),
        SampleRate = value.SampleRate,
        ChannelCount = value.ChannelCount,
        ChannelLayout = ProbeWireEnumText.ToWire(value.ChannelLayout),
        HasPolicyRelevantMetadata = value.HasPolicyRelevantMetadata,
    };

    private static MediaProbeFactsV1 FromWire(MediaProbeFactsWire value)
    {
        if (value.Streams is null)
        {
            throw new JsonException("Property facts.streams is required.");
        }

        if (value.HasChapters is null || value.HasGlobalMetadata is null || value.HasPolicyRelevantStreamMetadata is null)
        {
            throw new JsonException("Media probe fact boolean properties are required.");
        }

        return MapDomain("media probe facts", () => new MediaProbeFactsV1(
            ProbeWireEnumText.ParseContainer(value.Container),
            value.Streams.Select(FromWire).ToArray(),
            ProbeWireEnumText.ParseCompleteness(value.Completeness),
            value.HasChapters.Value,
            value.HasGlobalMetadata.Value,
            value.HasPolicyRelevantStreamMetadata.Value));
    }

    private static MediaStreamFactsV1 FromWire(MediaStreamFactsWire value)
    {
        if (value.Index is null || value.IsDefault is null || value.IsAttachedPicture is null || value.HasPolicyRelevantMetadata is null)
        {
            throw new JsonException("Media stream index and boolean properties are required.");
        }

        return MapDomain("media stream facts", () => new MediaStreamFactsV1(
            value.Index.Value,
            ProbeWireEnumText.ParseKind(value.Kind),
            ProbeWireEnumText.ParseCodec(value.Codec),
            ProbeWireEnumText.ParseProfile(value.Profile),
            value.IsDefault.Value,
            value.IsAttachedPicture.Value,
            ProbeWireEnumText.ParsePixelFormat(value.PixelFormat),
            value.BitDepth,
            value.Width,
            value.Height,
            ProbeWireEnumText.ParseColorTransfer(value.ColorTransfer),
            ProbeWireEnumText.ParseHdrState(value.HdrState),
            value.SampleRate,
            value.ChannelCount,
            ProbeWireEnumText.ParseChannelLayout(value.ChannelLayout),
            value.HasPolicyRelevantMetadata.Value));
    }
}
