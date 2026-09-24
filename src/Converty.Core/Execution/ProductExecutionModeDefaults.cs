using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Execution;

public static class ProductExecutionModeDefaults
{
    public static ConversionMode Resolve(PresetId presetId)
    {
        ArgumentNullException.ThrowIfNull(presetId);

        return presetId.Value switch
        {
            "video.mp4.h264" or "video.webm.vp9" or "extract.audio.mp3" => ConversionMode.Transcode,
            "audio.mp3" or "audio.flac" or "audio.m4a.aac" or "audio.opus" or "audio.ogg.vorbis" or "audio.wav" or
            "image.png" or "image.jpeg" or "image.webp" => ConversionMode.Transform,
            _ => throw new InvalidOperationException($"Unsupported product preset '{presetId.Value}'."),
        };
    }
}
