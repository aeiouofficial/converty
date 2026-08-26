using System.Collections.ObjectModel;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Presets;

public sealed class ProductPresetDefinition
{
    private readonly ReadOnlyCollection<string> _inputExtensions;
    private readonly HashSet<string> _inputExtensionSet;
    private readonly ReadOnlyCollection<string> _ffmpegArgumentsAfterInput;

    public ProductPresetDefinition(
        PresetId id,
        string displayName,
        string menuGroup,
        ProductMediaKind inputKind,
        IEnumerable<string> inputExtensions,
        string outputExtension,
        IEnumerable<string> ffmpegArgumentsAfterInput)
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

        ArgumentNullException.ThrowIfNull(ffmpegArgumentsAfterInput);
        string[] fixedArguments = ffmpegArgumentsAfterInput.ToArray();
        if (fixedArguments.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("FFmpeg preset arguments must be non-empty tokens.", nameof(ffmpegArgumentsAfterInput));
        }

        _inputExtensions = Array.AsReadOnly(extensions);
        _inputExtensionSet = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
        _ffmpegArgumentsAfterInput = Array.AsReadOnly(fixedArguments);
    }

    public PresetId Id { get; }
    public string DisplayName { get; }
    public string MenuGroup { get; }
    public ProductMediaKind InputKind { get; }
    public string OutputExtension { get; }
    public IReadOnlyList<string> InputExtensions => _inputExtensions;
    public IReadOnlyList<string> FfmpegArgumentsAfterInput => _ffmpegArgumentsAfterInput;

    public bool SupportsPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        string extension = Path.GetExtension(path);
        return !string.IsNullOrEmpty(extension) && _inputExtensionSet.Contains(extension);
    }

    public IReadOnlyList<string> BuildFfmpegArguments(string inputPath, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Input path is required.", nameof(inputPath));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path is required.", nameof(outputPath));
        }

        var arguments = new List<string>(12 + _ffmpegArgumentsAfterInput.Count)
        {
            "-hide_banner",
            "-loglevel",
            "error",
            "-nostdin",
            "-n",
            "-protocol_whitelist",
            "file,pipe",
            "-i",
            inputPath,
        };
        arguments.AddRange(_ffmpegArgumentsAfterInput);
        arguments.Add(outputPath);
        return arguments.AsReadOnly();
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
