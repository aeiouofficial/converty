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

    [Fact]
    public void VideoMp4PresetUsesExactDev20EncodingContract()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.mp4.h264"));

        Assert.Equal(".mp4", preset.OutputExtension);
        Assert.Equal(
            ["-map", "0:v:0?", "-map", "0:a:0?", "-c:v", "libx264", "-preset", "medium", "-crf", "23", "-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart"],
            preset.FfmpegArgumentsAfterInput);
    }

    [Fact]
    public void VideoWebmPresetUsesExactDev20EncodingContract()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.webm.vp9"));

        Assert.Equal(".webm", preset.OutputExtension);
        Assert.Equal(
            ["-map", "0:v:0?", "-map", "0:a:0?", "-c:v", "libvpx-vp9", "-crf", "32", "-b:v", "0", "-c:a", "libopus", "-b:a", "128k"],
            preset.FfmpegArgumentsAfterInput);
    }

    [Fact]
    public void ExtractAudioMp3PresetUsesExactDev20EncodingContract()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("extract.audio.mp3"));

        Assert.Equal(".mp3", preset.OutputExtension);
        Assert.Equal(
            ["-vn", "-c:a", "libmp3lame", "-b:a", "192k"],
            preset.FfmpegArgumentsAfterInput);
    }

    [Fact]
    public void VideoFfmpegArgumentsKeepUnicodeAndMetacharacterPathsAsIndependentTokens()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("video.mp4.h264"));
        const string input = @"C:\Media\Hör clip & semi; -dash [x].mov";
        const string output = @"C:\Media\Hör clip & semi; -dash [x].mp4";

        IReadOnlyList<string> arguments = preset.BuildFfmpegArguments(input, output);

        Assert.Equal(1, arguments.Count(argument => argument == input));
        Assert.Equal(1, arguments.Count(argument => argument == output));
        Assert.DoesNotContain(arguments, argument => argument.Contains("cmd.exe", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(arguments, argument => argument.Contains("powershell", StringComparison.OrdinalIgnoreCase));
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
    public void AudioMp3PresetUsesThe320kProductMvpBitrate()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.mp3"));

        Assert.Contains("libmp3lame", preset.FfmpegArgumentsAfterInput);
        Assert.Contains("320k", preset.FfmpegArgumentsAfterInput);
        Assert.DoesNotContain("192k", preset.FfmpegArgumentsAfterInput);
    }

    [Fact]
    public void AudioM4aAacPresetIsFixedAt256k()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.m4a.aac"));

        Assert.Equal("Convert to M4A (AAC)", preset.DisplayName);
        Assert.Equal(".m4a", preset.OutputExtension);
        Assert.Equal(
            ["-vn", "-c:a", "aac", "-b:a", "256k", "-movflags", "+faststart"],
            preset.FfmpegArgumentsAfterInput);
    }

    [Fact]
    public void AudioOpusPresetUsesFixedMusicEncodingParameters()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.opus"));

        Assert.Equal("Convert to Opus", preset.DisplayName);
        Assert.Equal(".opus", preset.OutputExtension);
        Assert.Equal(
            ["-vn", "-c:a", "libopus", "-b:a", "192k", "-vbr", "on", "-application", "audio"],
            preset.FfmpegArgumentsAfterInput);
    }

    [Fact]
    public void AudioOggVorbisPresetUsesFixedQualityParameters()
    {
        ProductPresetDefinition preset = ProductPresetRegistry.Default.GetRequired(PresetId.Parse("audio.ogg.vorbis"));

        Assert.Equal("Convert to Ogg Vorbis", preset.DisplayName);
        Assert.Equal(".ogg", preset.OutputExtension);
        Assert.Equal(
            ["-vn", "-c:a", "libvorbis", "-q:a", "6"],
            preset.FfmpegArgumentsAfterInput);
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
