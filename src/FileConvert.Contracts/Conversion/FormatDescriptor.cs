using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Conversion;

public sealed class FormatDescriptor
{
    public const int MaximumDisplayNameLength = 128;
    public const int MaximumExtensions = 32;

    public FormatDescriptor(
        FormatId id,
        FileFamilyId familyId,
        string displayName,
        string canonicalExtension,
        IEnumerable<string> extensions)
        : this(SchemaVersions.Current, id, familyId, displayName, canonicalExtension, extensions)
    {
    }

    public FormatDescriptor(
        int schemaVersion,
        FormatId id,
        FileFamilyId familyId,
        string displayName,
        string canonicalExtension,
        IEnumerable<string> extensions)
    {
        if (schemaVersion != SchemaVersions.Current)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Unsupported format descriptor schema version.");
        }

        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(familyId);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        var normalizedDisplayName = displayName.Trim();
        if (normalizedDisplayName.Length > MaximumDisplayNameLength)
        {
            throw new ArgumentException($"Display name must not exceed {MaximumDisplayNameLength} characters.", nameof(displayName));
        }

        SchemaVersion = schemaVersion;
        Id = id;
        FamilyId = familyId;
        DisplayName = normalizedDisplayName;
        CanonicalExtension = NormalizeExtension(canonicalExtension);

        ArgumentNullException.ThrowIfNull(extensions);
        var normalized = extensions.Select(NormalizeExtension).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one extension is required.", nameof(extensions));
        }

        if (normalized.Length > MaximumExtensions)
        {
            throw new ArgumentException($"A format may define at most {MaximumExtensions} extensions.", nameof(extensions));
        }

        if (!normalized.Contains(CanonicalExtension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Canonical extension must be included in the extension set.", nameof(canonicalExtension));
        }

        Extensions = Array.AsReadOnly(normalized);
    }

    public int SchemaVersion { get; }
    public FormatId Id { get; }
    public FileFamilyId FamilyId { get; }
    public string DisplayName { get; }
    public string CanonicalExtension { get; }
    public IReadOnlyList<string> Extensions { get; }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Extension is required.", nameof(extension));
        }

        var normalized = extension.Trim().ToLowerInvariant();
        if (!normalized.StartsWith('.', StringComparison.Ordinal) || normalized.Length < 2 || normalized.Length > 17)
        {
            throw new ArgumentException("Extensions must start with '.' and contain 1-16 extension characters.", nameof(extension));
        }

        if (normalized.Contains('/') || normalized.Contains('\\') || normalized[1..].Any(c => !char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException("Extension contains unsupported characters.", nameof(extension));
        }

        return normalized;
    }
}
