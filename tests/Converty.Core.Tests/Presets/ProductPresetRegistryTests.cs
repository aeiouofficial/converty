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
    public void AudioSelectionOffersAudioPresetsOnly()
    {
        IReadOnlyList<ProductPresetDefinition> presets = ProductPresetRegistry.Default.GetApplicable(
            [Path.Combine("work", "voice.wav")]);

        Assert.Equal(
            ["audio.mp3", "audio.flac"],
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
    public void AudioMp3PresetUsesThe320kProductMvpBitrate()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.mp3"));

        Assert.Contains("libmp3lame", preset.FfmpegArgumentsAfterInput);
        Assert.Contains("320k", preset.FfmpegArgumentsAfterInput);
        Assert.DoesNotContain("192k", preset.FfmpegArgumentsAfterInput);
    }

    [Fact]
    public void FfmpegArgumentsKeepPathsAsIndependentTokens()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.mp3"));
        const string input = @"C:\Media\track & echo injected; [x] -name.wav";
        const string output = @"C:\Media\track & echo injected; [x] -name.mp3";

        IReadOnlyList<string> arguments = preset.BuildFfmpegArguments(input, output);

        Assert.Contains(input, arguments);
        Assert.Contains(output, arguments);
        Assert.Equal(1, arguments.Count(argument => argument == input));
        Assert.Equal(1, arguments.Count(argument => argument == output));
        Assert.DoesNotContain(arguments, argument => argument.Contains("cmd.exe", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(arguments, argument => argument.Contains("powershell", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RegistryRejectsUnknownPresetId()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.unknown")));
    }
}
