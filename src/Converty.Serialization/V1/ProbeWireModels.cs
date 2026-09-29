namespace Converty.Serialization.V1;

internal sealed class MediaProbeResultWire
{
    public int SchemaVersion { get; set; }
    public string? Status { get; set; }
    public MediaProbeFactsWire? Facts { get; set; }
    public string? FailureReason { get; set; }
}

internal sealed class MediaProbeFactsWire
{
    public string? Container { get; set; }
    public MediaStreamFactsWire[]? Streams { get; set; }
    public string? Completeness { get; set; }
    public bool? HasChapters { get; set; }
    public bool? HasGlobalMetadata { get; set; }
    public bool? HasPolicyRelevantStreamMetadata { get; set; }
}

internal sealed class MediaStreamFactsWire
{
    public int? Index { get; set; }
    public string? Kind { get; set; }
    public string? Codec { get; set; }
    public string? Profile { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsAttachedPicture { get; set; }
    public string? PixelFormat { get; set; }
    public int? BitDepth { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? ColorTransfer { get; set; }
    public string? HdrState { get; set; }
    public int? SampleRate { get; set; }
    public int? ChannelCount { get; set; }
    public string? ChannelLayout { get; set; }
    public bool? HasPolicyRelevantMetadata { get; set; }
}
