using System.Collections.ObjectModel;
using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Provider.FFmpeg;

public sealed class FfmpegCompiledPreset
{
    private readonly ReadOnlyCollection<string> _inputPrefixTokens;
    private readonly ReadOnlyCollection<string> _outputSuffixTokens;

    internal FfmpegCompiledPreset(IEnumerable<string> inputPrefixTokens, IEnumerable<string> outputSuffixTokens)
    {
        ArgumentNullException.ThrowIfNull(inputPrefixTokens);
        ArgumentNullException.ThrowIfNull(outputSuffixTokens);

        string[] prefix = inputPrefixTokens.ToArray();
        string[] suffix = outputSuffixTokens.ToArray();
        if (prefix.Length == 0 || suffix.Length == 0 || prefix.Any(string.IsNullOrWhiteSpace) || suffix.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("Compiled FFmpeg token vectors must contain only fixed non-empty tokens.");
        }

        _inputPrefixTokens = Array.AsReadOnly(prefix);
        _outputSuffixTokens = Array.AsReadOnly(suffix);
    }

    public IReadOnlyList<string> InputPrefixTokens => _inputPrefixTokens;
    public IReadOnlyList<string> OutputSuffixTokens => _outputSuffixTokens;
}

public static class FfmpegPresetCompiler
{
    private static readonly string[] FixedInputPrefix =
    [
        "-hide_banner",
        "-loglevel", "error",
        "-nostdin",
        "-n",
        "-protocol_whitelist", "file",
        "-i",
    ];

    public static FfmpegCompiledPreset Compile(PresetId presetId, ConversionMode mode)
    {
        ArgumentNullException.ThrowIfNull(presetId);

        string[] suffix = (presetId.Value, mode) switch
        {
            ("video.mp4.h264", ConversionMode.Remux) =>
            [
                "-map", "0:v:0",
                "-map", "0:a:0?",
                "-map_metadata", "-1",
                "-map_chapters", "-1",
                "-c:v", "copy",
                "-c:a", "copy",
                "-movflags", "+faststart",
            ],
            ("video.mp4.h264", ConversionMode.Transcode) =>
            [
                "-map", "0:v:0",
                "-map", "0:a:0?",
                "-map_metadata", "-1",
                "-map_chapters", "-1",
                "-c:v", "libx264",
                "-preset", "medium",
                "-crf", "23",
                "-pix_fmt", "yuv420p",
                "-c:a", "aac",
                "-b:a", "192k",
                "-ar", "48000",
                "-ac", "2",
                "-movflags", "+faststart",
            ],
            ("video.webm.vp9", ConversionMode.Remux) =>
            [
                "-map", "0:v:0",
                "-map", "0:a:0?",
                "-map_metadata", "-1",
                "-map_chapters", "-1",
                "-c:v", "copy",
                "-c:a", "copy",
            ],
            ("video.webm.vp9", ConversionMode.Transcode) =>
            [
                "-map", "0:v:0",
                "-map", "0:a:0?",
                "-map_metadata", "-1",
                "-map_chapters", "-1",
                "-c:v", "libvpx-vp9",
                "-crf", "32",
                "-b:v", "0",
                "-pix_fmt", "yuv420p",
                "-c:a", "libopus",
                "-b:a", "128k",
                "-ar", "48000",
                "-ac", "2",
            ],
            ("extract.audio.mp3", ConversionMode.Remux) =>
            [
                "-map", "0:a:0",
                "-map_metadata", "-1",
                "-map_chapters", "-1",
                "-vn",
                "-c:a", "copy",
            ],
            ("extract.audio.mp3", ConversionMode.Transcode) =>
            [
                "-map", "0:a:0",
                "-map_metadata", "-1",
                "-map_chapters", "-1",
                "-vn",
                "-c:a", "libmp3lame",
                "-b:a", "192k",
                "-ar", "44100",
                "-ac", "2",
            ],
            ("audio.mp3", ConversionMode.Transform) =>
                ["-vn", "-c:a", "libmp3lame", "-b:a", "320k"],
            ("audio.flac", ConversionMode.Transform) =>
                ["-vn", "-c:a", "flac"],
            ("audio.m4a.aac", ConversionMode.Transform) =>
                ["-vn", "-c:a", "aac", "-b:a", "256k", "-movflags", "+faststart"],
            ("audio.opus", ConversionMode.Transform) =>
                ["-vn", "-c:a", "libopus", "-b:a", "192k", "-vbr", "on", "-application", "audio"],
            ("audio.ogg.vorbis", ConversionMode.Transform) =>
                ["-vn", "-c:a", "libvorbis", "-q:a", "6"],
            ("audio.wav", ConversionMode.Transform) =>
                ["-vn", "-c:a", "pcm_s16le"],
            ("image.png", ConversionMode.Transform) =>
                ["-frames:v", "1", "-c:v", "png"],
            ("image.jpeg", ConversionMode.Transform) =>
                ["-frames:v", "1", "-c:v", "mjpeg", "-q:v", "2"],
            ("image.webp", ConversionMode.Transform) =>
                ["-frames:v", "1", "-c:v", "libwebp", "-quality", "85"],
            _ => throw new InvalidOperationException(
                $"Unsupported fixed FFmpeg preset/mode tuple: {presetId.Value}/{mode}."),
        };

        return new FfmpegCompiledPreset(FixedInputPrefix, suffix);
    }

    internal static ConversionMode ResolveCurrentProductMode(PresetId presetId)
    {
        ArgumentNullException.ThrowIfNull(presetId);
        return presetId.Value switch
        {
            "video.mp4.h264" or "video.webm.vp9" or "extract.audio.mp3" => ConversionMode.Transcode,
            "audio.mp3" or "audio.flac" or "audio.m4a.aac" or "audio.opus" or "audio.ogg.vorbis" or "audio.wav" or
            "image.png" or "image.jpeg" or "image.webp" => ConversionMode.Transform,
            _ => throw new InvalidOperationException($"Unsupported fixed FFmpeg preset: {presetId.Value}."),
        };
    }
}
