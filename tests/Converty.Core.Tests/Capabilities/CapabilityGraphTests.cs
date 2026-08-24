using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Capabilities;

namespace Converty.Core.Tests.Capabilities;

public sealed class CapabilityGraphTests
{
    [Fact]
    public void FindReturnsCapabilitiesInDeterministicPriorityOrder()
    {
        var source = FormatId.Parse("audio.wav");
        var target = FormatId.Parse("audio.mp3");
        var low = new CapabilityDescriptor(ProviderId.Parse("provider.low"), source, target, ConversionMode.Transcode, 10);
        var high = new CapabilityDescriptor(ProviderId.Parse("provider.high"), source, target, ConversionMode.Transcode, 100);
        var graph = new CapabilityGraph([low, high]);

        var result = graph.Find(source, target);

        Assert.Equal(new[] { high, low }, result);
    }

    [Fact]
    public void ConstructorRejectsDuplicateCapabilityIdentity()
    {
        var source = FormatId.Parse("audio.wav");
        var target = FormatId.Parse("audio.mp3");
        var capability = new CapabilityDescriptor(ProviderId.Parse("provider.one"), source, target, ConversionMode.Transcode, 100);

        Assert.Throws<ArgumentException>(() => new CapabilityGraph([capability, capability]));
    }

    [Fact]
    public void FindReturnsEmptyForUnsupportedRoute()
    {
        var graph = new CapabilityGraph([]);

        var result = graph.Find(FormatId.Parse("audio.wav"), FormatId.Parse("audio.flac"));

        Assert.Empty(result);
    }
}
