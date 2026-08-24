using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;

namespace FileConvert.Core.Registry;

public sealed class FormatRegistry
{
    private readonly IReadOnlyList<FormatDescriptor> _formats;
    private readonly IReadOnlyDictionary<FormatId, FormatDescriptor> _byId;
    private readonly IReadOnlyDictionary<string, FormatDescriptor> _byExtension;

    public FormatRegistry(IEnumerable<FormatDescriptor> formats)
    {
        ArgumentNullException.ThrowIfNull(formats);
        var snapshot = formats.ToArray();
        if (snapshot.Any(format => format is null))
        {
            throw new ArgumentException("Formats must not contain null values.", nameof(formats));
        }

        var byId = new Dictionary<FormatId, FormatDescriptor>();
        var byExtension = new Dictionary<string, FormatDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var format in snapshot)
        {
            if (!byId.TryAdd(format.Id, format))
            {
                throw new ArgumentException($"Duplicate format ID: {format.Id}.", nameof(formats));
            }

            foreach (var extension in format.Extensions)
            {
                if (!byExtension.TryAdd(extension, format))
                {
                    throw new ArgumentException($"Extension {extension} is registered to more than one format.", nameof(formats));
                }
            }
        }

        _formats = Array.AsReadOnly(snapshot);
        _byId = new System.Collections.ObjectModel.ReadOnlyDictionary<FormatId, FormatDescriptor>(byId);
        _byExtension = new System.Collections.ObjectModel.ReadOnlyDictionary<string, FormatDescriptor>(byExtension);
    }

    public IReadOnlyList<FormatDescriptor> Formats => _formats;

    public FormatDescriptor? Find(FormatId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _byId.TryGetValue(id, out var format) ? format : null;
    }

    public FormatDescriptor? FindByExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return null;
        }

        var normalized = extension.Trim();
        if (!normalized.StartsWith('.', StringComparison.Ordinal))
        {
            normalized = "." + normalized;
        }

        return _byExtension.TryGetValue(normalized, out var format) ? format : null;
    }
}
