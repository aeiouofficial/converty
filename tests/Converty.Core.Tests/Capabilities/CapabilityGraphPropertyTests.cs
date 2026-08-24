using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Capabilities;

namespace Converty.Core.Tests.Capabilities;

public sealed class CapabilityGraphPropertyTests
{
    [Fact]
    public void FindOrderIsStableAcrossSeededInputPermutations()
    {
        var random = new Random(0x4C1A551);
        var source = FormatId.Parse("audio.wav");
        var target = FormatId.Parse("audio.mp3");
        var canonical = new[]
        {
            new CapabilityDescriptor(ProviderId.Parse("provider.a"), source, target, ConversionMode.Transcode, 100),
            new CapabilityDescriptor(ProviderId.Parse("provider.b"), source, target, ConversionMode.Remux, 100),
            new CapabilityDescriptor(ProviderId.Parse("provider.c"), source, target, ConversionMode.Transcode, 80),
            new CapabilityDescriptor(ProviderId.Parse("provider.d"), source, target, ConversionMode.Copy, 20),
        };
        var expected = new CapabilityGraph(canonical).Find(source, target).ToArray();
        for (var iteration = 0; iteration < 500; iteration++)
        {
            var permutation = canonical.OrderBy(_ => random.Next()).ToArray();
            Assert.Equal(expected, new CapabilityGraph(permutation).Find(source, target).ToArray());
        }
    }
}
