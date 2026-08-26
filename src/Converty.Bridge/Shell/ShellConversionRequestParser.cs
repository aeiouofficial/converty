using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Presets;

namespace Converty.Bridge.Shell;

public static class ShellConversionRequestParser
{
    private const string PresetSwitch = "--preset";
    private const string PathSeparator = "--";

    public static ShellConversionRequest Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Count < 4
            || !string.Equals(args[0], PresetSwitch, StringComparison.Ordinal)
            || !string.Equals(args[2], PathSeparator, StringComparison.Ordinal))
        {
            throw new ArgumentException("Expected: --preset <preset-id> -- <absolute path> [...].", nameof(args));
        }

        PresetId presetId = PresetId.Parse(args[1]);
        _ = ProductPresetRegistry.Default.GetRequired(presetId);

        int fileCount = args.Count - 3;
        if (fileCount is < 1 or > ConversionRequest.MaximumFiles)
        {
            throw new ArgumentException(
                $"Explorer request must contain 1-{ConversionRequest.MaximumFiles} selected files.",
                nameof(args));
        }

        var paths = new string[fileCount];
        for (int index = 0; index < fileCount; index++)
        {
            string path = args[index + 3];
            if (string.IsNullOrWhiteSpace(path)
                || path.Length > ConversionRequest.MaximumPathLength
                || !Path.IsPathFullyQualified(path))
            {
                throw new ArgumentException("Explorer paths must be non-empty fully-qualified filesystem paths.", nameof(args));
            }

            paths[index] = path;
        }

        return new ShellConversionRequest(presetId, Array.AsReadOnly(paths));
    }
}
