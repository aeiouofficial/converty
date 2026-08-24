using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Contracts.Providers;

/// <summary>
/// Data-only provider descriptor contract. Provider execution is intentionally absent from this interface.
/// </summary>
public interface IConverterProvider
{
    ProviderId Id { get; }
    IReadOnlyList<CapabilityDescriptor> Capabilities { get; }
}
