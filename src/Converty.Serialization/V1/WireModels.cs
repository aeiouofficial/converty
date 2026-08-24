namespace Converty.Serialization.V1;

internal sealed class ConversionRequestWire
{
    public int SchemaVersion { get; set; }
    public string? RequestId { get; set; }
    public string? Action { get; set; }
    public string[]? Files { get; set; }
    public string? TargetFormat { get; set; }
    public string? PresetId { get; set; }
}

internal sealed class ConversionPresetWire
{
    public int SchemaVersion { get; set; }
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? FamilyId { get; set; }
    public string? OutputFormat { get; set; }
    public string? PreferredProvider { get; set; }
    public SortedDictionary<string, string>? Options { get; set; }
}

internal sealed class CapabilityDescriptorWire
{
    public int SchemaVersion { get; set; }
    public string? ProviderId { get; set; }
    public string? SourceFormat { get; set; }
    public string? TargetFormat { get; set; }
    public string? Mode { get; set; }
    public int Priority { get; set; }
}

internal sealed class FormatDescriptorWire
{
    public int SchemaVersion { get; set; }
    public string? Id { get; set; }
    public string? FamilyId { get; set; }
    public string? DisplayName { get; set; }
    public string? CanonicalExtension { get; set; }
    public string[]? Extensions { get; set; }
}

internal sealed class ProbedFileDescriptorWire
{
    public string? Path { get; set; }
    public string? FamilyId { get; set; }
    public string? FormatId { get; set; }
    public long Length { get; set; }
}

internal sealed class ConversionPlanWire
{
    public int SchemaVersion { get; set; }
    public string? RequestId { get; set; }
    public ProbedFileDescriptorWire? Source { get; set; }
    public string? TargetFormat { get; set; }
    public string? ProviderId { get; set; }
    public string? Mode { get; set; }
    public string? PresetId { get; set; }
}

internal sealed class JobStatusSnapshotWire
{
    public int SchemaVersion { get; set; }
    public string? JobId { get; set; }
    public string? RequestId { get; set; }
    public string? State { get; set; }
    public double? Progress { get; set; }
    public string? Message { get; set; }
}
