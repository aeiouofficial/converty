using Converty.Contracts.Identifiers;
using Converty.Core.Capabilities;
using Converty.FakeProviders;

namespace Converty.Core.Tests;

public sealed class FakeProviderTests
{
    [Fact]
    public void DefaultCatalogExposesIndependentAudioImageAndVideoRoutes()
    {
        var providers = FakeProviderCatalog.CreateDefault();
        var graph = new CapabilityGraph(providers.SelectMany(p => p.Capabilities));

        Assert.NotEmpty(graph.Find(FormatId.Parse("audio.wav"), FormatId.Parse("audio.mp3")));
        Assert.NotEmpty(graph.Find(FormatId.Parse("image.tiff"), FormatId.Parse("image.png")));
        Assert.NotEmpty(graph.Find(FormatId.Parse("video.mov"), FormatId.Parse("video.mp4")));
        Assert.Empty(graph.Find(FormatId.Parse("image.tiff"), FormatId.Parse("audio.mp3")));
    }
}
