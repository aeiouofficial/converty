using System.Reflection;
using Converty.Contracts.Identifiers;
using Converty.Core.Presets;

namespace Converty.Core.Tests.Presets;

public sealed class ProductPresetRegistryTests
{
    [Fact]
    public void VideoSelectionOffersVideoAndExtractAudioPresetsOnly()
    {
        IReadOnlyList<ProductPresetDefinition> presets = ProductPresetRegistry.Default.GetApplicable(
            [Path.Combine("work", "holiday.mov")]);

        Assert.Contains(presets, preset => preset.Id == PresetId.Parse("video.mp4.h264"));
        Assert.Contains(presets, preset => preset.Id == PresetId.Parse("video.webm.vp9"));
        Assert.Contains(presets, preset => preset.Id == PresetId.Parse("extract.audio.mp3"));
        Assert.DoesNotContain(presets, preset => preset.Id.Value.StartsWith("image.", StringComparison.Ordinal));
    }

    [Fact]
    public void VideoPresetsSupportExactlyTheNineAdvertisedSourceExtensions()
    {
        string[] expected = [".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".mpeg", ".mpg", ".wmv"];

        foreach (string id in new[] { "video.mp4.h264", "video.webm.vp9", "extract.audio.mp3" })
        {
            ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse(id));

            Assert.Equal(expected, preset.InputExtensions);
            Assert.All(expected, extension => Assert.True(preset.SupportsPath("clip" + extension)));
        }
    }

    [Theory]
    [InlineData("video.mp4.h264", "Convert to MP4", "Video", ProductMediaKind.Video, ".mp4")]
    [InlineData("video.webm.vp9", "Convert to WebM", "Video", ProductMediaKind.Video, ".webm")]
    [InlineData("extract.audio.mp3", "Extract Audio to MP3", "Extract Audio", ProductMediaKind.Video, ".mp3")]
    [InlineData("audio.mp3", "Convert to MP3", "Audio", ProductMediaKind.Audio, ".mp3")]
    [InlineData("audio.flac", "Convert to FLAC", "Audio", ProductMediaKind.Audio, ".flac")]
    [InlineData("audio.m4a.aac", "Convert to M4A (AAC)", "Audio", ProductMediaKind.Audio, ".m4a")]
    [InlineData("audio.opus", "Convert to Opus", "Audio", ProductMediaKind.Audio, ".opus")]
    [InlineData("audio.ogg.vorbis", "Convert to Ogg Vorbis", "Audio", ProductMediaKind.Audio, ".ogg")]
    [InlineData("audio.wav", "Convert to WAV", "Audio", ProductMediaKind.Audio, ".wav")]
    [InlineData("image.png", "Convert to PNG", "Image", ProductMediaKind.Image, ".png")]
    [InlineData("image.jpeg", "Convert to JPEG", "Image", ProductMediaKind.Image, ".jpg")]
    [InlineData("image.webp", "Convert to WebP", "Image", ProductMediaKind.Image, ".webp")]
    public void RegistryRetainsOnlyAdvertisedProductSemantics(
        string id,
        string displayName,
        string menuGroup,
        ProductMediaKind inputKind,
        string outputExtension)
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse(id));

        Assert.Equal(displayName, preset.DisplayName);
        Assert.Equal(menuGroup, preset.MenuGroup);
        Assert.Equal(inputKind, preset.InputKind);
        Assert.Equal(outputExtension, preset.OutputExtension);
    }

    [Fact]
    public void ProductPresetDefinitionExposesNoFfmpegExecutionSurface()
    {
        string[] publicMemberNames = typeof(ProductPresetDefinition)
            .GetMembers(BindingFlags.Instance | BindingFlags.Public)
            .Select(member => member.Name)
            .ToArray();

        Assert.DoesNotContain("FfmpegArgumentsAfterInput", publicMemberNames);
        Assert.DoesNotContain("BuildFfmpegArguments", publicMemberNames);
        Assert.DoesNotContain(publicMemberNames, name => name.Contains("Ffmpeg", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AudioSelectionOffersExpandedFixedAudioPresetsOnly()
    {
        IReadOnlyList<ProductPresetDefinition> presets = ProductPresetRegistry.Default.GetApplicable(
            [Path.Combine("work", "voice.wav")]);

        Assert.Equal(
            ["audio.mp3", "audio.flac", "audio.m4a.aac", "audio.opus", "audio.ogg.vorbis"],
            presets.Select(preset => preset.Id.Value).ToArray());
    }

    [Fact]
    public void ImageSelectionOffersImagePresetsAndHidesIdentityOutput()
    {
        IReadOnlyList<ProductPresetDefinition> presets = ProductPresetRegistry.Default.GetApplicable(
            [Path.Combine("work", "cover.png")]);

        Assert.Equal(
            ["image.jpeg", "image.webp"],
            presets.Select(preset => preset.Id.Value).ToArray());
    }

    [Fact]
    public void MixedFamiliesHaveNoSharedPreset()
    {
        IReadOnlyList<ProductPresetDefinition> presets = ProductPresetRegistry.Default.GetApplicable(
            [Path.Combine("work", "clip.mov"), Path.Combine("work", "voice.wav")]);

        Assert.Empty(presets);
    }

    [Fact]
    public void UnsupportedExtensionHasNoPreset()
    {
        Assert.Empty(ProductPresetRegistry.Default.GetApplicable([Path.Combine("work", "notes.txt")]));
    }

    [Fact]
    public void RegistryRejectsUnknownPresetId()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.unknown")));
    }
}
