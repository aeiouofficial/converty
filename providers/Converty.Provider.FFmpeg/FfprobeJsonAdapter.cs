using System.Globalization;
using System.Text.Json;
using Converty.Contracts.Conversion;

namespace Converty.Provider.FFmpeg;

public static class FfprobeJsonAdapter
{
    private const int MaximumBackendStringCharacters = 256;

    public static MediaProbeFactsV1 Parse(string inputPath, string rawJson)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || !Path.IsPathFullyQualified(inputPath))
        {
            throw new ArgumentException("Probe input path must be fully qualified.", nameof(inputPath));
        }
        ArgumentNullException.ThrowIfNull(rawJson);

        using JsonDocument document = JsonDocument.Parse(rawJson, new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 16,
        });

        JsonElement root = document.RootElement;
        RequireObject(root, "root");
        RejectDuplicatePropertiesRecursively(root);

        JsonElement streamsElement = RequireProperty(root, "streams", JsonValueKind.Array);
        JsonElement formatElement = RequireProperty(root, "format", JsonValueKind.Object);
        JsonElement chaptersElement = root.TryGetProperty("chapters", out JsonElement chapters)
            ? RequireKind(chapters, JsonValueKind.Array, "chapters")
            : default;

        int streamCount = streamsElement.GetArrayLength();
        if (streamCount > MediaProbeFactsV1.MaximumStreams)
        {
            throw new JsonException($"FFprobe returned more than {MediaProbeFactsV1.MaximumStreams} streams.");
        }

        string? formatName = GetOptionalBoundedString(formatElement, "format_name");
        MediaContainerId container = MapContainer(Path.GetExtension(inputPath), formatName);
        bool hasGlobalMetadata = HasNonEmptyObject(formatElement, "tags");
        bool hasChapters = chaptersElement.ValueKind == JsonValueKind.Array && chaptersElement.GetArrayLength() > 0;

        var mappedStreams = new List<MediaStreamFactsV1>(streamCount);
        bool complete = container != MediaContainerId.Unknown;
        bool hasPolicyRelevantStreamMetadata = false;

        foreach (JsonElement streamElement in streamsElement.EnumerateArray())
        {
            RequireObject(streamElement, "stream");
            MediaStreamFactsV1 stream = MapStream(streamElement, out bool streamComplete, out bool streamMetadata);
            mappedStreams.Add(stream);
            complete &= streamComplete;
            hasPolicyRelevantStreamMetadata |= streamMetadata;
        }

        return new MediaProbeFactsV1(
            container,
            mappedStreams,
            complete ? MediaProbeCompleteness.Complete : MediaProbeCompleteness.Incomplete,
            hasChapters,
            hasGlobalMetadata,
            hasPolicyRelevantStreamMetadata);
    }

    private static MediaStreamFactsV1 MapStream(
        JsonElement element,
        out bool complete,
        out bool hasPolicyRelevantMetadata)
    {
        int index = GetRequiredInt32(element, "index");
        string? typeText = GetOptionalBoundedString(element, "codec_type");
        string? codecText = GetOptionalBoundedString(element, "codec_name");
        string? profileText = GetOptionalBoundedString(element, "profile");
        MediaStreamKind kind = MapStreamKind(typeText);
        MediaCodecId codec = MapCodec(codecText);
        MediaProfileId profile = MapProfile(profileText);
        bool isDefault = GetDispositionFlag(element, "default");
        bool isAttachedPicture = GetDispositionFlag(element, "attached_pic");
        hasPolicyRelevantMetadata = HasNonEmptyObject(element, "tags") || HasNonEmptyArray(element, "side_data_list");

        MediaPixelFormatId pixelFormat = MediaPixelFormatId.Unknown;
        int? bitDepth = null;
        int? width = null;
        int? height = null;
        MediaColorTransferId colorTransfer = MediaColorTransferId.Unknown;
        MediaHdrState hdrState = MediaHdrState.Unknown;
        int? sampleRate = null;
        int? channelCount = null;
        MediaAudioChannelLayoutId channelLayout = MediaAudioChannelLayoutId.Unknown;

        if (kind == MediaStreamKind.Video)
        {
            string? pixelText = GetOptionalBoundedString(element, "pix_fmt");
            pixelFormat = MapPixelFormat(pixelText);
            bitDepth = GetOptionalNumericString(element, "bits_per_raw_sample") ??
                GetOptionalInt32(element, "bits_per_sample");
            if (!bitDepth.HasValue && pixelFormat == MediaPixelFormatId.Yuv420p)
            {
                bitDepth = 8;
            }
            width = GetOptionalInt32(element, "width");
            height = GetOptionalInt32(element, "height");
            string? transferText = GetOptionalBoundedString(element, "color_transfer");
            colorTransfer = MapColorTransfer(transferText);
            hdrState = colorTransfer switch
            {
                MediaColorTransferId.Bt709 => MediaHdrState.Sdr,
                MediaColorTransferId.Smpte2084 or MediaColorTransferId.AribStdB67 => MediaHdrState.Hdr,
                _ => MediaHdrState.Unknown,
            };

            complete = codec != MediaCodecId.Unknown
                && pixelFormat != MediaPixelFormatId.Unknown
                && bitDepth.HasValue
                && width.HasValue
                && height.HasValue
                && colorTransfer != MediaColorTransferId.Unknown
                && hdrState != MediaHdrState.Unknown;
        }
        else if (kind == MediaStreamKind.Audio)
        {
            sampleRate = GetOptionalNumericString(element, "sample_rate") ?? GetOptionalInt32(element, "sample_rate");
            channelCount = GetOptionalInt32(element, "channels");
            channelLayout = MapChannelLayout(GetOptionalBoundedString(element, "channel_layout"), channelCount);
            complete = codec != MediaCodecId.Unknown
                && sampleRate.HasValue
                && channelCount.HasValue
                && channelLayout != MediaAudioChannelLayoutId.Unknown;
        }
        else
        {
            complete = kind != MediaStreamKind.Unknown && codec != MediaCodecId.Unknown;
        }

        try
        {
            return new MediaStreamFactsV1(
                index,
                kind,
                codec,
                profile,
                isDefault,
                isAttachedPicture,
                pixelFormat,
                bitDepth,
                width,
                height,
                colorTransfer,
                hdrState,
                sampleRate,
                channelCount,
                channelLayout,
                hasPolicyRelevantMetadata);
        }
        catch (ArgumentException error)
        {
            throw new JsonException("FFprobe returned contradictory or out-of-range stream facts.", error);
        }
    }

    private static MediaContainerId MapContainer(string extension, string? formatName)
    {
        string ext = extension.ToLowerInvariant();
        string[] names = (formatName ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        bool Has(string value) => names.Contains(value, StringComparer.OrdinalIgnoreCase);

        return ext switch
        {
            ".mp4" or ".m4v" when Has("mov") || Has("mp4") => MediaContainerId.Mp4,
            ".mov" when Has("mov") || Has("mp4") => MediaContainerId.Mov,
            ".mkv" when Has("matroska") || Has("webm") => MediaContainerId.Matroska,
            ".webm" when Has("matroska") || Has("webm") => MediaContainerId.WebM,
            ".avi" when Has("avi") => MediaContainerId.Avi,
            ".mpeg" or ".mpg" when Has("mpeg") => MediaContainerId.Mpeg,
            ".wmv" when Has("asf") => MediaContainerId.Wmv,
            ".mp3" when Has("mp3") => MediaContainerId.Mp3,
            _ => MediaContainerId.Unknown,
        };
    }

    private static MediaStreamKind MapStreamKind(string? value) => value?.ToLowerInvariant() switch
    {
        "video" => MediaStreamKind.Video,
        "audio" => MediaStreamKind.Audio,
        "subtitle" => MediaStreamKind.Subtitle,
        "data" => MediaStreamKind.Data,
        "attachment" => MediaStreamKind.Attachment,
        _ => MediaStreamKind.Unknown,
    };

    private static MediaCodecId MapCodec(string? value) => value?.ToLowerInvariant() switch
    {
        "h264" => MediaCodecId.H264,
        "vp9" => MediaCodecId.Vp9,
        "mpeg4" => MediaCodecId.Mpeg4,
        "mpeg2video" => MediaCodecId.Mpeg2Video,
        "wmv2" => MediaCodecId.Wmv2,
        "aac" => MediaCodecId.Aac,
        "opus" => MediaCodecId.Opus,
        "mp3" => MediaCodecId.Mp3,
        "mp2" => MediaCodecId.Mp2,
        "wmav2" => MediaCodecId.Wmav2,
        _ => MediaCodecId.Unknown,
    };

    private static MediaProfileId MapProfile(string? value)
    {
        string normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        return normalized switch
        {
            "baseline" or "constrained baseline" => MediaProfileId.H264Baseline,
            "main" => MediaProfileId.H264Main,
            "high" => MediaProfileId.H264High,
            "profile 0" or "0" => MediaProfileId.Vp9Profile0,
            "" => MediaProfileId.Unknown,
            _ => MediaProfileId.OtherKnown,
        };
    }

    private static MediaPixelFormatId MapPixelFormat(string? value) => value?.ToLowerInvariant() switch
    {
        "yuv420p" => MediaPixelFormatId.Yuv420p,
        _ => MediaPixelFormatId.Unknown,
    };

    private static MediaColorTransferId MapColorTransfer(string? value) => value?.ToLowerInvariant() switch
    {
        "bt709" => MediaColorTransferId.Bt709,
        "smpte2084" => MediaColorTransferId.Smpte2084,
        "arib-std-b67" => MediaColorTransferId.AribStdB67,
        "smpte170m" or "bt470bg" => MediaColorTransferId.OtherKnown,
        _ => MediaColorTransferId.Unknown,
    };

    private static MediaAudioChannelLayoutId MapChannelLayout(string? value, int? channels)
    {
        string normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        if (normalized == "mono" || channels == 1)
        {
            return MediaAudioChannelLayoutId.Mono;
        }
        if (normalized == "stereo" || channels == 2)
        {
            return MediaAudioChannelLayoutId.Stereo;
        }
        if (channels is > 2)
        {
            return MediaAudioChannelLayoutId.Multichannel;
        }
        return string.IsNullOrEmpty(normalized)
            ? MediaAudioChannelLayoutId.Unknown
            : MediaAudioChannelLayoutId.OtherKnown;
    }

    private static JsonElement RequireProperty(JsonElement element, string name, JsonValueKind kind)
    {
        if (!element.TryGetProperty(name, out JsonElement value))
        {
            throw new JsonException($"FFprobe JSON is missing required property '{name}'.");
        }
        return RequireKind(value, kind, name);
    }

    private static JsonElement RequireKind(JsonElement element, JsonValueKind kind, string name)
    {
        if (element.ValueKind != kind)
        {
            throw new JsonException($"FFprobe property '{name}' has an unexpected JSON type.");
        }
        return element;
    }

    private static void RequireObject(JsonElement element, string name) =>
        _ = RequireKind(element, JsonValueKind.Object, name);

    private static int GetRequiredInt32(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int result))
        {
            throw new JsonException($"FFprobe property '{name}' must be a bounded integer.");
        }
        return result;
    }

    private static int? GetOptionalInt32(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int result))
        {
            throw new JsonException($"FFprobe property '{name}' must be a bounded integer when present.");
        }
        return result;
    }

    private static int? GetOptionalNumericString(JsonElement element, string name)
    {
        string? value = GetOptionalBoundedString(element, name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int result))
        {
            throw new JsonException($"FFprobe property '{name}' must contain a bounded integer string.");
        }
        return result;
    }

    private static string? GetOptionalBoundedString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"FFprobe property '{name}' must be a string when present.");
        }
        string text = value.GetString() ?? string.Empty;
        if (text.Length > MaximumBackendStringCharacters)
        {
            throw new JsonException($"FFprobe property '{name}' exceeds its bounded string limit.");
        }
        return text;
    }

    private static bool GetDispositionFlag(JsonElement element, string name)
    {
        if (!element.TryGetProperty("disposition", out JsonElement disposition) || disposition.ValueKind != JsonValueKind.Object)
        {
            return false;
        }
        if (!disposition.TryGetProperty(name, out JsonElement value))
        {
            return false;
        }
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int flag) || flag is < 0 or > 1)
        {
            throw new JsonException($"FFprobe disposition '{name}' must be 0 or 1.");
        }
        return flag == 1;
    }

    private static bool HasNonEmptyObject(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value)
        && value.ValueKind == JsonValueKind.Object
        && value.EnumerateObject().Any();

    private static bool HasNonEmptyArray(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value)
        && value.ValueKind == JsonValueKind.Array
        && value.GetArrayLength() > 0;

    private static void RejectDuplicatePropertiesRecursively(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    throw new JsonException($"FFprobe JSON contains duplicate property '{property.Name}'.");
                }
                if (property.Name.Length > MaximumBackendStringCharacters)
                {
                    throw new JsonException("FFprobe JSON contains an oversized property name.");
                }
                RejectDuplicatePropertiesRecursively(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                RejectDuplicatePropertiesRecursively(item);
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            string text = element.GetString() ?? string.Empty;
            if (text.Length > MaximumBackendStringCharacters)
            {
                throw new JsonException("FFprobe JSON contains an oversized string value.");
            }
        }
    }
}
