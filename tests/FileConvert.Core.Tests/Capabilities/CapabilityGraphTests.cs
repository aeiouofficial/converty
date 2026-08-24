using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;
using FileConvert.Core.Capabilities;

namespace FileConvert.Core.Tests.Capabilities;

public sealed class CapabilityGraphTests
{
    [Fact]
    public void Find_returns_capabilities_in_deterministic_priority_order()
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
    public void Constructor_rejects_duplicate_capability_identity()
    {
        var source = FormatId.Parse("audio.wav");
        var target = FormatId.Parse("audio.mp3");
        var capability = new CapabilityDescriptor(ProviderId.Parse("provider.one"), source, target, ConversionMode.Transcode, 100);

        Assert.Throws<ArgumentException>(() => new CapabilityGraph([capability, capability]));
    }

    [Fact]
    public void Find_returns_empty_for_unsupported_route()
    {
        var graph = new CapabilityGraph([]);

        var result = graph.Find(FormatId.Parse("audio.wav"), FormatId.Parse("audio.flac"));

        Assert.Empty(result);
    }
}
