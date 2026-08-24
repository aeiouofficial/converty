using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;
using FileConvert.Contracts.Providers;

namespace FileConvert.FakeProviders;

public static class FakeProviderCatalog
{
    public static IReadOnlyList<IConverterProvider> CreateDefault()
    {
        var audioId = ProviderId.Parse("fake.audio");
        var imageId = ProviderId.Parse("fake.image");
        var videoId = ProviderId.Parse("fake.video");

        var audio = new FakeConverterProvider(audioId,
        [
            new CapabilityDescriptor(audioId, FormatId.Parse("audio.wav"), FormatId.Parse("audio.mp3"), ConversionMode.Transcode, 100),
            new CapabilityDescriptor(audioId, FormatId.Parse("audio.wav"), FormatId.Parse("audio.flac"), ConversionMode.Transcode, 100),
            new CapabilityDescriptor(audioId, FormatId.Parse("audio.flac"), FormatId.Parse("audio.mp3"), ConversionMode.Transcode, 100),
        ]);

        var image = new FakeConverterProvider(imageId,
        [
            new CapabilityDescriptor(imageId, FormatId.Parse("image.tiff"), FormatId.Parse("image.png"), ConversionMode.Transform, 100),
            new CapabilityDescriptor(imageId, FormatId.Parse("image.jpeg"), FormatId.Parse("image.png"), ConversionMode.Transform, 100),
        ]);

        var video = new FakeConverterProvider(videoId,
        [
            new CapabilityDescriptor(videoId, FormatId.Parse("video.mov"), FormatId.Parse("video.mp4"), ConversionMode.Remux, 100),
            new CapabilityDescriptor(videoId, FormatId.Parse("video.mkv"), FormatId.Parse("video.mp4"), ConversionMode.Remux, 100),
        ]);

        return Array.AsReadOnly<IConverterProvider>(new IConverterProvider[] { audio, image, video });
    }
}
