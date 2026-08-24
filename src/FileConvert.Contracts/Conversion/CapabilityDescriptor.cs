using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Conversion;

public sealed record CapabilityDescriptor
{
    public CapabilityDescriptor(
        ProviderId providerId,
        FormatId sourceFormat,
        FormatId targetFormat,
        ConversionMode mode,
        int priority)
        : this(SchemaVersions.Current, providerId, sourceFormat, targetFormat, mode, priority)
    {
    }

    public CapabilityDescriptor(
        int schemaVersion,
        ProviderId providerId,
        FormatId sourceFormat,
        FormatId targetFormat,
        ConversionMode mode,
        int priority)
    {
        if (schemaVersion != SchemaVersions.Current)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Unsupported provider capability schema version.");
        }

        ArgumentNullException.ThrowIfNull(providerId);
        ArgumentNullException.ThrowIfNull(sourceFormat);
        ArgumentNullException.ThrowIfNull(targetFormat);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), "Unsupported conversion mode.");
        }

        if (priority is < 0 or > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be between 0 and 1000.");
        }

        SchemaVersion = schemaVersion;
        ProviderId = providerId;
        SourceFormat = sourceFormat;
        TargetFormat = targetFormat;
        Mode = mode;
        Priority = priority;
    }

    public int SchemaVersion { get; }
    public ProviderId ProviderId { get; }
    public FormatId SourceFormat { get; }
    public FormatId TargetFormat { get; }
    public ConversionMode Mode { get; }
    public int Priority { get; }
}
