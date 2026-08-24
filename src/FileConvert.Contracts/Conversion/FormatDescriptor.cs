using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Conversion;

/// <summary>
/// Canonical, engine-independent description of a file format.
/// </summary>
public sealed class FormatDescriptor
{
    public const int CurrentSchemaVersion = 1;
    public const int MaximumDisplayNameLength = 128;
    public const int MaximumExtensions = 32;

    public FormatDescriptor(
        FormatId id,
        FileFamilyId familyId,
        string displayName,
        string canonicalExtension,
        IEnumerable<string> extensions,
        int schemaVersion = CurrentSchemaVersion)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(extensions);

        if (schemaVersion != CurrentSchemaVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), $"Only schema version {CurrentSchemaVersion} is supported.");
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > MaximumDisplayNameLength)
        {
            throw new ArgumentException($"Display name must contain 1-{MaximumDisplayNameLength} characters.", nameof(displayName));
        }

        var normalizedCanonical = NormalizeExtension(canonicalExtension);
        var normalizedExtensions = extensions
            .Select(NormalizeExtension)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (normalizedExtensions.Length is 0 or > MaximumExtensions)
        {
            throw new ArgumentException($"Formats must define 1-{MaximumExtensions} unique extensions.", nameof(extensions));
        }

        if (!normalizedExtensions.Contains(normalizedCanonical, StringComparer.Ordinal))
        {
            throw new ArgumentException("Canonical extension must be included in extensions.", nameof(canonicalExtension));
        }

        SchemaVersion = schemaVersion;
        Id = id;
        FamilyId = familyId;
        DisplayName = displayName.Trim();
        CanonicalExtension = normalizedCanonical;
        Extensions = Array.AsReadOnly(normalizedExtensions);
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
        if (!normalized.StartsWith(".", StringComparison.Ordinal) || normalized.Length < 2 || normalized.Length > 17)
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
