using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;

namespace FileConvert.Contracts.Providers;

/// <summary>
/// Data-only provider descriptor contract. Provider execution is intentionally absent from this interface.
/// </summary>
public interface IConverterProvider
{
    ProviderId Id { get; }
    IReadOnlyList<CapabilityDescriptor> Capabilities { get; }
}
