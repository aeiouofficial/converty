using System.Collections.ObjectModel;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Presets;

public sealed class ProductPresetDefinition
{
    private readonly ReadOnlyCollection<string> _inputExtensions;
    private readonly HashSet<string> _inputExtensionSet;

    public ProductPresetDefinition(
        PresetId id,
        string displayName,
        string menuGroup,
        ProductMediaKind inputKind,
        IEnumerable<string> inputExtensions,
        string outputExtension)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        DisplayName = RequireText(displayName, nameof(displayName));
        MenuGroup = RequireText(menuGroup, nameof(menuGroup));
        if (!Enum.IsDefined(inputKind))
        {
            throw new ArgumentOutOfRangeException(nameof(inputKind));
        }

        InputKind = inputKind;
        OutputExtension = NormalizeExtension(outputExtension, nameof(outputExtension));

        ArgumentNullException.ThrowIfNull(inputExtensions);
        string[] extensions = inputExtensions
            .Select(extension => NormalizeExtension(extension, nameof(inputExtensions)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (extensions.Length == 0)
        {
            throw new ArgumentException("At least one input extension is required.", nameof(inputExtensions));
        }

        _inputExtensions = Array.AsReadOnly(extensions);
        _inputExtensionSet = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
    }

    public PresetId Id { get; }
    public string DisplayName { get; }
    public string MenuGroup { get; }
    public ProductMediaKind InputKind { get; }
    public string OutputExtension { get; }
    public IReadOnlyList<string> InputExtensions => _inputExtensions;

    public bool SupportsPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        string extension = Path.GetExtension(path);
        return !string.IsNullOrEmpty(extension) && _inputExtensionSet.Contains(extension);
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeExtension(string extension, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Extension is required.", parameterName);
        }

        string normalized = extension.Trim().ToLowerInvariant();
        if (normalized[0] != '.')
        {
            normalized = "." + normalized;
        }

        if (normalized.Length is < 2 or > 17 || normalized[1..].Any(character => !char.IsAsciiLetterOrDigit(character)))
        {
            throw new ArgumentException("Extension must contain 1-16 ASCII alphanumeric characters.", parameterName);
        }

        return normalized;
    }
}
