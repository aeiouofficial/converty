namespace Converty.Contracts.Conversion;

public sealed class MediaStreamFactsV1
{
    public const int MaximumStreamIndex = 1023;
    public const int MaximumDimension = 32768;
    public const int MaximumBitDepth = 16;
    public const int MaximumSampleRate = 768000;
    public const int MaximumChannels = 64;

    public MediaStreamFactsV1(
        int index,
        MediaStreamKind kind,
        MediaCodecId codec,
        MediaProfileId profile,
        bool isDefault,
        bool isAttachedPicture,
        MediaPixelFormatId pixelFormat,
        int? bitDepth,
        int? width,
        int? height,
        MediaColorTransferId colorTransfer,
        MediaHdrState hdrState,
        int? sampleRate,
        int? channelCount,
        MediaAudioChannelLayoutId channelLayout,
        bool hasPolicyRelevantMetadata)
    {
        if (index is < 0 or > MaximumStreamIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        ValidateEnum(kind, nameof(kind));
        ValidateEnum(codec, nameof(codec));
        ValidateEnum(profile, nameof(profile));
        ValidateEnum(pixelFormat, nameof(pixelFormat));
        ValidateEnum(colorTransfer, nameof(colorTransfer));
        ValidateEnum(hdrState, nameof(hdrState));
        ValidateEnum(channelLayout, nameof(channelLayout));

        ValidateOptionalRange(bitDepth, 1, MaximumBitDepth, nameof(bitDepth));
        ValidateOptionalRange(width, 1, MaximumDimension, nameof(width));
        ValidateOptionalRange(height, 1, MaximumDimension, nameof(height));
        ValidateOptionalRange(sampleRate, 1, MaximumSampleRate, nameof(sampleRate));
        ValidateOptionalRange(channelCount, 1, MaximumChannels, nameof(channelCount));

        if (width.HasValue != height.HasValue)
        {
            throw new ArgumentException("Video dimensions must be both present or both absent.");
        }

        if (isAttachedPicture && kind != MediaStreamKind.Video)
        {
            throw new ArgumentException("Attached-picture disposition is valid only for Video streams.", nameof(isAttachedPicture));
        }

        bool hasVideoFacts = pixelFormat != MediaPixelFormatId.Unknown
            || bitDepth.HasValue
            || width.HasValue
            || height.HasValue
            || colorTransfer != MediaColorTransferId.Unknown
            || hdrState != MediaHdrState.Unknown;
        bool hasAudioFacts = sampleRate.HasValue
            || channelCount.HasValue
            || channelLayout != MediaAudioChannelLayoutId.Unknown;

        if (kind != MediaStreamKind.Video && hasVideoFacts)
        {
            throw new ArgumentException("Video-only facts are valid only for Video streams.");
        }

        if (kind != MediaStreamKind.Audio && hasAudioFacts)
        {
            throw new ArgumentException("Audio-only facts are valid only for Audio streams.");
        }

        if (hdrState == MediaHdrState.Sdr
            && colorTransfer is MediaColorTransferId.Smpte2084 or MediaColorTransferId.AribStdB67)
        {
            throw new ArgumentException("SDR state contradicts an HDR transfer characteristic.", nameof(hdrState));
        }

        Index = index;
        Kind = kind;
        Codec = codec;
        Profile = profile;
        IsDefault = isDefault;
        IsAttachedPicture = isAttachedPicture;
        PixelFormat = pixelFormat;
        BitDepth = bitDepth;
        Width = width;
        Height = height;
        ColorTransfer = colorTransfer;
        HdrState = hdrState;
        SampleRate = sampleRate;
        ChannelCount = channelCount;
        ChannelLayout = channelLayout;
        HasPolicyRelevantMetadata = hasPolicyRelevantMetadata;
    }

    public int Index { get; }
    public MediaStreamKind Kind { get; }
    public MediaCodecId Codec { get; }
    public MediaProfileId Profile { get; }
    public bool IsDefault { get; }
    public bool IsAttachedPicture { get; }
    public MediaPixelFormatId PixelFormat { get; }
    public int? BitDepth { get; }
    public int? Width { get; }
    public int? Height { get; }
    public MediaColorTransferId ColorTransfer { get; }
    public MediaHdrState HdrState { get; }
    public int? SampleRate { get; }
    public int? ChannelCount { get; }
    public MediaAudioChannelLayoutId ChannelLayout { get; }
    public bool HasPolicyRelevantMetadata { get; }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Unsupported media probe enum value.");
        }
    }

    private static void ValidateOptionalRange(int? value, int minimum, int maximum, string parameterName)
    {
        if (value.HasValue && (value.Value < minimum || value.Value > maximum))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
