using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Contracts.Providers;

namespace Converty.FakeProviders;

public sealed class FakeConverterProvider : IConverterProvider
{
    public FakeConverterProvider(ProviderId id, IEnumerable<CapabilityDescriptor> capabilities)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ArgumentNullException.ThrowIfNull(capabilities);
        var snapshot = capabilities.ToArray();
        if (snapshot.Any(capability => capability.ProviderId != id))
        {
            throw new ArgumentException("All fake-provider capabilities must use the provider's ID.", nameof(capabilities));
        }

        Capabilities = Array.AsReadOnly(snapshot);
    }

    public ProviderId Id { get; }
    public IReadOnlyList<CapabilityDescriptor> Capabilities { get; }
}
