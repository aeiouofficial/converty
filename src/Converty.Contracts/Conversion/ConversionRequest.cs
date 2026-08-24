using Converty.Contracts.Identifiers;

namespace Converty.Contracts.Conversion;

public sealed class ConversionRequest
{
    public const int MaximumFiles = 1024;
    public const int MaximumPathLength = 32767;

    public ConversionRequest(
        int schemaVersion,
        Guid requestId,
        ConversionAction action,
        IEnumerable<string> files,
        FormatId? targetFormat,
        PresetId? presetId)
    {
        if (schemaVersion != SchemaVersions.Current)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Unsupported conversion request schema version.");
        }

        if (requestId == Guid.Empty)
        {
            throw new ArgumentException("Request ID must not be empty.", nameof(requestId));
        }

        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(nameof(action), "Unsupported conversion action.");
        }

        ArgumentNullException.ThrowIfNull(files);
        var snapshot = files.ToArray();
        if (snapshot.Length is < 1 or > MaximumFiles)
        {
            throw new ArgumentException($"A conversion request must contain 1-{MaximumFiles} files.", nameof(files));
        }

        if (snapshot.Any(path => string.IsNullOrWhiteSpace(path) || path.Length > MaximumPathLength || path.Contains('\0')))
        {
            throw new ArgumentException($"File paths must contain 1-{MaximumPathLength} characters.", nameof(files));
        }

        switch (action)
        {
            case ConversionAction.ConvertUsingDefault when targetFormat is not null || presetId is not null:
                throw new ArgumentException("Default conversion must not provide a target format or preset.", nameof(action));
            case ConversionAction.ConvertToFormat when targetFormat is null:
                throw new ArgumentException("ConvertToFormat requires a target format.", nameof(targetFormat));
            case ConversionAction.ConvertToFormat when presetId is not null:
                throw new ArgumentException("ConvertToFormat must not provide a preset.", nameof(presetId));
            case ConversionAction.ConvertWithPreset when presetId is null:
                throw new ArgumentException("ConvertWithPreset requires a preset ID.", nameof(presetId));
            case ConversionAction.ConvertWithPreset when targetFormat is not null:
                throw new ArgumentException("ConvertWithPreset must not provide a target format; the preset owns its output format.", nameof(targetFormat));
        }

        SchemaVersion = schemaVersion;
        RequestId = requestId;
        Action = action;
        Files = Array.AsReadOnly(snapshot);
        TargetFormat = targetFormat;
        PresetId = presetId;
    }

    public int SchemaVersion { get; }
    public Guid RequestId { get; }
    public ConversionAction Action { get; }
    public IReadOnlyList<string> Files { get; }
    public FormatId? TargetFormat { get; }
    public PresetId? PresetId { get; }
}
