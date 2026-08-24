using Converty.Contracts.Identifiers;

namespace Converty.Contracts.Conversion;

public sealed class ConversionPreset
{
    public const int MaximumDisplayNameLength = 128;
    public const int MaximumOptions = 128;
    public const int MaximumOptionValueLength = 256;

    public ConversionPreset(
        int schemaVersion,
        PresetId id,
        string displayName,
        FileFamilyId familyId,
        FormatId outputFormat,
        ProviderId? preferredProvider,
        IReadOnlyDictionary<string, string>? options = null)
    {
        if (schemaVersion != SchemaVersions.Current)
        {
            throw new ArgumentOutOfRangeException(nameof(schemaVersion), "Unsupported preset schema version.");
        }

        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(outputFormat);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        var normalizedDisplayName = displayName.Trim();
        if (normalizedDisplayName.Length > MaximumDisplayNameLength)
        {
            throw new ArgumentException($"Display name must not exceed {MaximumDisplayNameLength} characters.", nameof(displayName));
        }

        if (options is not null && options.Count > MaximumOptions)
        {
            throw new ArgumentException($"Preset options must not exceed {MaximumOptions} entries.", nameof(options));
        }

        var optionSnapshot = new Dictionary<string, string>(StringComparer.Ordinal);
        if (options is not null)
        {
            foreach (var pair in options)
            {
                if (!IdentifierRules.IsValid(pair.Key) || string.IsNullOrWhiteSpace(pair.Value) || pair.Value.Length > MaximumOptionValueLength)
                {
                    throw new ArgumentException(
                        $"Preset options require canonical keys and 1-{MaximumOptionValueLength} character non-empty values.",
                        nameof(options));
                }

                optionSnapshot.Add(pair.Key, pair.Value);
            }
        }

        SchemaVersion = schemaVersion;
        Id = id;
        DisplayName = normalizedDisplayName;
        FamilyId = familyId;
        OutputFormat = outputFormat;
        PreferredProvider = preferredProvider;
        Options = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(optionSnapshot);
    }

    public int SchemaVersion { get; }
    public PresetId Id { get; }
    public string DisplayName { get; }
    public FileFamilyId FamilyId { get; }
    public FormatId OutputFormat { get; }
    public ProviderId? PreferredProvider { get; }
    public IReadOnlyDictionary<string, string> Options { get; }
}
