using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Conversion;

/// <summary>Declarative plan only. Engine argument construction belongs to isolated provider workers.</summary>
public sealed class ConversionPlan
{
    public ConversionPlan(
        int schemaVersion,
        Guid requestId,
        ProbedFileDescriptor source,
        FormatId targetFormat,
        ProviderId providerId,
        ConversionMode mode,
        PresetId? presetId)
    {
        if (schemaVersion != SchemaVersions.Current)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Unsupported conversion plan schema version.");
        }

        if (requestId == Guid.Empty)
        {
            throw new ArgumentException("Request ID must not be empty.", nameof(requestId));
        }

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(targetFormat);
        ArgumentNullException.ThrowIfNull(providerId);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), "Unsupported conversion mode.");
        }

        SchemaVersion = schemaVersion;
        RequestId = requestId;
        Source = source;
        TargetFormat = targetFormat;
        ProviderId = providerId;
        Mode = mode;
        PresetId = presetId;
    }

    public int SchemaVersion { get; }
    public Guid RequestId { get; }
    public ProbedFileDescriptor Source { get; }
    public FormatId TargetFormat { get; }
    public ProviderId ProviderId { get; }
    public ConversionMode Mode { get; }
    public PresetId? PresetId { get; }
}
