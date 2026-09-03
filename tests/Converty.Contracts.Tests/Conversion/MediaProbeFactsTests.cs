using System.Reflection;
using Converty.Contracts.Conversion;

namespace Converty.Contracts.Tests.Conversion;

public sealed class MediaProbeFactsTests
{
    [Fact]
    public void Dev21ProbeContractSurfaceExistsBeforeImplementation()
    {
        Assembly contracts = typeof(ProbedFileDescriptor).Assembly;

        Assert.NotNull(contracts.GetType("Converty.Contracts.Conversion.MediaProbeFactsV1"));
        Assert.NotNull(contracts.GetType("Converty.Contracts.Conversion.MediaProbeResultV1"));
        Assert.NotNull(contracts.GetType("Converty.Contracts.Conversion.MediaStreamFactsV1"));

        ConstructorInfo? additiveConstructor = typeof(ProbedFileDescriptor)
            .GetConstructors()
            .SingleOrDefault(constructor => constructor.GetParameters().Length == 5);
        Assert.NotNull(additiveConstructor);

        ConstructorInfo? legacyConstructor = typeof(ProbedFileDescriptor)
            .GetConstructors()
            .SingleOrDefault(constructor => constructor.GetParameters().Length == 4);
        Assert.NotNull(legacyConstructor);
    }
}
