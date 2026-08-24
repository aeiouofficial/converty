using FileConvert.Contracts.Identifiers;
using FileConvert.Core.Capabilities;
using FileConvert.FakeProviders;

namespace FileConvert.Core.Tests;

public sealed class FakeProviderTests
{
    [Fact]
    public void Default_catalog_exposes_independent_audio_image_and_video_routes()
    {
        var providers = FakeProviderCatalog.CreateDefault();
        var graph = new CapabilityGraph(providers.SelectMany(p => p.Capabilities));

        Assert.NotEmpty(graph.Find(FormatId.Parse("audio.wav"), FormatId.Parse("audio.mp3")));
        Assert.NotEmpty(graph.Find(FormatId.Parse("image.tiff"), FormatId.Parse("image.png")));
        Assert.NotEmpty(graph.Find(FormatId.Parse("video.mov"), FormatId.Parse("video.mp4")));
        Assert.Empty(graph.Find(FormatId.Parse("image.tiff"), FormatId.Parse("audio.mp3")));
    }
}
