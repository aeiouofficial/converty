using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Capabilities;

public sealed class CapabilityGraph
{
    private readonly IReadOnlyList<CapabilityDescriptor> _capabilities;

    public CapabilityGraph(IEnumerable<CapabilityDescriptor> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        var snapshot = capabilities.ToArray();
        if (snapshot.Any(c => c is null))
        {
            throw new ArgumentException("Capabilities must not contain null values.", nameof(capabilities));
        }

        var duplicate = snapshot
            .GroupBy(c => new CapabilityKey(c.ProviderId, c.SourceFormat, c.TargetFormat, c.Mode))
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException($"Duplicate capability identity: {duplicate.Key}.", nameof(capabilities));
        }

        _capabilities = Array.AsReadOnly(snapshot
            .OrderByDescending(c => c.Priority)
            .ThenBy(c => c.ProviderId.Value, StringComparer.Ordinal)
            .ThenBy(c => c.Mode)
            .ToArray());
    }

    public IReadOnlyList<CapabilityDescriptor> All => _capabilities;

    public IReadOnlyList<CapabilityDescriptor> Find(FormatId sourceFormat, FormatId targetFormat)
    {
        ArgumentNullException.ThrowIfNull(sourceFormat);
        ArgumentNullException.ThrowIfNull(targetFormat);

        return Array.AsReadOnly(_capabilities
            .Where(c => c.SourceFormat == sourceFormat && c.TargetFormat == targetFormat)
            .ToArray());
    }

    private sealed record CapabilityKey(
        ProviderId ProviderId,
        FormatId SourceFormat,
        FormatId TargetFormat,
        ConversionMode Mode);
}
