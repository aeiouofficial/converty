using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Provider.FFmpeg;

namespace Converty.Core.Tests.Provider;

public sealed class FfmpegPresetCompilerTests
{
    [Theory]
    [InlineData("video.mp4.h264", ConversionMode.Remux)]
    [InlineData("video.mp4.h264", ConversionMode.Transcode)]
    [InlineData("video.webm.vp9", ConversionMode.Remux)]
    [InlineData("video.webm.vp9", ConversionMode.Transcode)]
    [InlineData("extract.audio.mp3", ConversionMode.Remux)]
    [InlineData("extract.audio.mp3", ConversionMode.Transcode)]
    [InlineData("audio.mp3", ConversionMode.Transform)]
    [InlineData("audio.flac", ConversionMode.Transform)]
    [InlineData("audio.m4a.aac", ConversionMode.Transform)]
    [InlineData("audio.opus", ConversionMode.Transform)]
    [InlineData("audio.ogg.vorbis", ConversionMode.Transform)]
    [InlineData("audio.wav", ConversionMode.Transform)]
    [InlineData("image.png", ConversionMode.Transform)]
    [InlineData("image.jpeg", ConversionMode.Transform)]
    [InlineData("image.webp", ConversionMode.Transform)]
    public void CompilerAcceptsOnlyExplicitSupportedTupleSet(string presetId, ConversionMode mode)
    {
        FfmpegCompiledPreset compiled = FfmpegPresetCompiler.Compile(PresetId.Parse(presetId), mode);

        Assert.NotEmpty(compiled.InputPrefixTokens);
        Assert.NotEmpty(compiled.OutputSuffixTokens);
        Assert.Equal("-i", compiled.InputPrefixTokens[^1]);
    }

    [Theory]
    [InlineData("video.mp4.h264", ConversionMode.Copy)]
    [InlineData("video.mp4.h264", ConversionMode.Transform)]
    [InlineData("video.webm.vp9", ConversionMode.Copy)]
    [InlineData("extract.audio.mp3", ConversionMode.Copy)]
    [InlineData("audio.mp3", ConversionMode.Remux)]
    [InlineData("audio.mp3", ConversionMode.Transcode)]
    [InlineData("image.png", ConversionMode.Remux)]
    [InlineData("video.unknown", ConversionMode.Transcode)]
    public void UnsupportedTupleRejectsBeforeAnyProcessSurface(string presetId, ConversionMode mode)
    {
        Assert.Throws<InvalidOperationException>(() =>
            FfmpegPresetCompiler.Compile(PresetId.Parse(presetId), mode));
    }

    [Fact]
    public void Mp4TranscodeOwnsExactCompatibilityAndStrippingPolicy()
    {
        FfmpegCompiledPreset compiled = FfmpegPresetCompiler.Compile(
            PresetId.Parse("video.mp4.h264"),
            ConversionMode.Transcode);

        Assert.Equal(
            ["-hide_banner", "-loglevel", "error", "-nostdin", "-n", "-protocol_whitelist", "file", "-i"],
            compiled.InputPrefixTokens);
        Assert.Equal(
            ["-map", "0:v:0", "-map", "0:a:0?", "-map_metadata", "-1", "-map_chapters", "-1", "-c:v", "libx264", "-preset", "medium", "-crf", "23", "-pix_fmt", "yuv420p", "-c:a", "aac", "-b:a", "192k", "-ar", "48000", "-ac", "2", "-movflags", "+faststart"],
            compiled.OutputSuffixTokens);
    }

    [Fact]
    public void WebmTranscodeOwnsExactCompatibilityAndStrippingPolicy()
    {
        FfmpegCompiledPreset compiled = FfmpegPresetCompiler.Compile(
            PresetId.Parse("video.webm.vp9"),
            ConversionMode.Transcode);

        Assert.Equal(
            ["-map", "0:v:0", "-map", "0:a:0?", "-map_metadata", "-1", "-map_chapters", "-1", "-c:v", "libvpx-vp9", "-crf", "32", "-b:v", "0", "-pix_fmt", "yuv420p", "-c:a", "libopus", "-b:a", "128k", "-ar", "48000", "-ac", "2"],
            compiled.OutputSuffixTokens);
    }

    [Fact]
    public void VideoRemuxUsesExplicitMappingAndStreamCopyOnly()
    {
        FfmpegCompiledPreset compiled = FfmpegPresetCompiler.Compile(
            PresetId.Parse("video.mp4.h264"),
            ConversionMode.Remux);

        Assert.Equal(
            ["-map", "0:v:0", "-map", "0:a:0?", "-map_metadata", "-1", "-map_chapters", "-1", "-c:v", "copy", "-c:a", "copy", "-movflags", "+faststart"],
            compiled.OutputSuffixTokens);
        Assert.DoesNotContain("libx264", compiled.OutputSuffixTokens);
    }

    [Fact]
    public void ExtractMp3TranscodeUsesFixedAudioCompatibilityProfile()
    {
        FfmpegCompiledPreset compiled = FfmpegPresetCompiler.Compile(
            PresetId.Parse("extract.audio.mp3"),
            ConversionMode.Transcode);

        Assert.Equal(
            ["-map", "0:a:0", "-map_metadata", "-1", "-map_chapters", "-1", "-vn", "-c:a", "libmp3lame", "-b:a", "192k", "-ar", "44100", "-ac", "2"],
            compiled.OutputSuffixTokens);
    }

    [Fact]
    public void ExistingAudioTransformProfileRemainsFixed()
    {
        FfmpegCompiledPreset compiled = FfmpegPresetCompiler.Compile(
            PresetId.Parse("audio.mp3"),
            ConversionMode.Transform);

        Assert.Equal(["-vn", "-c:a", "libmp3lame", "-b:a", "320k"], compiled.OutputSuffixTokens);
    }

    [Fact]
    public void CompiledTokenVectorsAreReadOnlyAndContainNoHardwareAcceleration()
    {
        FfmpegCompiledPreset compiled = FfmpegPresetCompiler.Compile(
            PresetId.Parse("video.mp4.h264"),
            ConversionMode.Transcode);

        IList<string> prefix = Assert.IsAssignableFrom<IList<string>>(compiled.InputPrefixTokens);
        IList<string> suffix = Assert.IsAssignableFrom<IList<string>>(compiled.OutputSuffixTokens);
        Assert.Throws<NotSupportedException>(() => prefix.Add("-hwaccel"));
        Assert.Throws<NotSupportedException>(() => suffix.Add("cuda"));

        string allTokens = string.Join("\n", compiled.InputPrefixTokens.Concat(compiled.OutputSuffixTokens)).ToLowerInvariant();
        foreach (string forbidden in new[] { "-hwaccel", "nvenc", "cuda", "qsv", "d3d11va", "videotoolbox", "amf" })
        {
            Assert.DoesNotContain(forbidden, allTokens, StringComparison.Ordinal);
        }
    }
}
