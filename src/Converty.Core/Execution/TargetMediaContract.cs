using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Execution;

public sealed class TargetMediaContract
{
    private TargetMediaContract(
        PresetId presetId,
        ConversionMode mode,
        MediaContainerId container,
        bool expectsVideo,
        MediaCodecId videoCodec,
        MediaPixelFormatId videoPixelFormat,
        int? videoBitDepth,
        MediaColorTransferId videoColorTransfer,
        MediaHdrState videoHdrState,
        bool expectsAudio,
        MediaCodecId audioCodec,
        int? audioSampleRate,
        int? audioChannelCount,
        MediaAudioChannelLayoutId audioChannelLayout,
        bool hasChapters,
        bool hasGlobalMetadata,
        bool hasPolicyRelevantStreamMetadata)
    {
        PresetId = presetId;
        Mode = mode;
        Container = container;
        ExpectsVideo = expectsVideo;
        VideoCodec = videoCodec;
        VideoPixelFormat = videoPixelFormat;
        VideoBitDepth = videoBitDepth;
        VideoColorTransfer = videoColorTransfer;
        VideoHdrState = videoHdrState;
        ExpectsAudio = expectsAudio;
        AudioCodec = audioCodec;
        AudioSampleRate = audioSampleRate;
        AudioChannelCount = audioChannelCount;
        AudioChannelLayout = audioChannelLayout;
        HasChapters = hasChapters;
        HasGlobalMetadata = hasGlobalMetadata;
        HasPolicyRelevantStreamMetadata = hasPolicyRelevantStreamMetadata;
    }

    public PresetId PresetId { get; }
    public ConversionMode Mode { get; }
    public MediaContainerId Container { get; }
    public bool ExpectsVideo { get; }
    public MediaCodecId VideoCodec { get; }
    public MediaPixelFormatId VideoPixelFormat { get; }
    public int? VideoBitDepth { get; }
    public MediaColorTransferId VideoColorTransfer { get; }
    public MediaHdrState VideoHdrState { get; }
    public bool ExpectsAudio { get; }
    public MediaCodecId AudioCodec { get; }
    public int? AudioSampleRate { get; }
    public int? AudioChannelCount { get; }
    public MediaAudioChannelLayoutId AudioChannelLayout { get; }
    public bool HasChapters { get; }
    public bool HasGlobalMetadata { get; }
    public bool HasPolicyRelevantStreamMetadata { get; }

    public static TargetMediaContract ForPlan(
        PresetId presetId,
        ConversionMode mode,
        MediaProbeFactsV1 sourceFacts)
    {
        ArgumentNullException.ThrowIfNull(presetId);
        ArgumentNullException.ThrowIfNull(sourceFacts);
        if (mode is not (ConversionMode.Copy or ConversionMode.Remux or ConversionMode.Transcode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), "Video targets require Copy, Remux, or Transcode mode.");
        }

        MediaStreamFactsV1[] videoStreams = sourceFacts.Streams
            .Where(stream => stream.Kind == MediaStreamKind.Video && !stream.IsAttachedPicture)
            .ToArray();
        MediaStreamFactsV1[] audioStreams = sourceFacts.Streams
            .Where(stream => stream.Kind == MediaStreamKind.Audio)
            .ToArray();
        if (audioStreams.Length > 1)
        {
            throw new InvalidOperationException("Target contract cannot be built from ambiguous Audio streams.");
        }

        MediaStreamFactsV1? sourceAudio = audioStreams.SingleOrDefault();
        bool preserveSourceMetadata = mode == ConversionMode.Copy;

        return presetId.Value switch
        {
            "video.mp4.h264" => CreateVideoTarget(
                presetId,
                mode,
                sourceFacts,
                RequireSingleSourceVideo(videoStreams),
                sourceAudio,
                MediaContainerId.Mp4,
                MediaCodecId.H264,
                MediaCodecId.Aac,
                transcodeAudioSampleRate: 48000,
                preserveSourceMetadata),
            "video.webm.vp9" => CreateVideoTarget(
                presetId,
                mode,
                sourceFacts,
                RequireSingleSourceVideo(videoStreams),
                sourceAudio,
                MediaContainerId.WebM,
                MediaCodecId.Vp9,
                MediaCodecId.Opus,
                transcodeAudioSampleRate: 48000,
                preserveSourceMetadata),
            "extract.audio.mp3" => CreateAudioTarget(presetId, mode, sourceAudio),
            _ => throw new InvalidOperationException($"Unsupported Video target preset '{presetId.Value}'."),
        };
    }

    private static MediaStreamFactsV1 RequireSingleSourceVideo(MediaStreamFactsV1[] videoStreams)
    {
        if (videoStreams.Length != 1)
        {
            throw new InvalidOperationException("Video target contract requires exactly one primary source Video stream.");
        }

        return videoStreams[0];
    }

    private static TargetMediaContract CreateVideoTarget(
        PresetId presetId,
        ConversionMode mode,
        MediaProbeFactsV1 sourceFacts,
        MediaStreamFactsV1 sourceVideo,
        MediaStreamFactsV1? sourceAudio,
        MediaContainerId transcodeOrRemuxContainer,
        MediaCodecId transcodeVideoCodec,
        MediaCodecId transcodeAudioCodec,
        int transcodeAudioSampleRate,
        bool preserveSourceMetadata)
    {
        bool expectsAudio = sourceAudio is not null;
        bool sourcePreserving = mode == ConversionMode.Copy || mode == ConversionMode.Remux;

        MediaContainerId container = mode == ConversionMode.Copy
            ? sourceFacts.Container
            : transcodeOrRemuxContainer;
        MediaCodecId videoCodec = sourcePreserving ? sourceVideo.Codec : transcodeVideoCodec;
        MediaPixelFormatId pixelFormat = sourcePreserving ? sourceVideo.PixelFormat : MediaPixelFormatId.Yuv420p;
        int? bitDepth = sourcePreserving ? sourceVideo.BitDepth : 8;
        MediaColorTransferId colorTransfer = sourcePreserving
            ? sourceVideo.ColorTransfer
            : MediaColorTransferId.Bt709;
        MediaHdrState hdrState = sourcePreserving ? sourceVideo.HdrState : MediaHdrState.Sdr;

        MediaCodecId audioCodec = MediaCodecId.Unknown;
        int? audioSampleRate = null;
        int? audioChannelCount = null;
        MediaAudioChannelLayoutId audioChannelLayout = MediaAudioChannelLayoutId.Unknown;
        if (sourceAudio is not null)
        {
            if (sourcePreserving)
            {
                audioCodec = sourceAudio.Codec;
                audioSampleRate = sourceAudio.SampleRate;
                audioChannelCount = sourceAudio.ChannelCount;
                audioChannelLayout = sourceAudio.ChannelLayout;
            }
            else
            {
                audioCodec = transcodeAudioCodec;
                audioSampleRate = transcodeAudioSampleRate;
                audioChannelCount = 2;
                audioChannelLayout = MediaAudioChannelLayoutId.Stereo;
            }
        }

        return new TargetMediaContract(
            presetId,
            mode,
            container,
            expectsVideo: true,
            videoCodec,
            pixelFormat,
            bitDepth,
            colorTransfer,
            hdrState,
            expectsAudio,
            audioCodec,
            audioSampleRate,
            audioChannelCount,
            audioChannelLayout,
            preserveSourceMetadata && sourceFacts.HasChapters,
            preserveSourceMetadata && sourceFacts.HasGlobalMetadata,
            preserveSourceMetadata && sourceFacts.HasPolicyRelevantStreamMetadata);
    }

    private static TargetMediaContract CreateAudioTarget(
        PresetId presetId,
        ConversionMode mode,
        MediaStreamFactsV1? sourceAudio)
    {
        if (mode == ConversionMode.Copy)
        {
            throw new InvalidOperationException("Audio extraction does not support Copy mode.");
        }
        if (sourceAudio is null)
        {
            throw new InvalidOperationException("Audio extraction target requires exactly one source Audio stream.");
        }

        bool sourcePreserving = mode == ConversionMode.Remux;
        MediaCodecId codec = sourcePreserving ? sourceAudio.Codec : MediaCodecId.Mp3;
        int? sampleRate = sourcePreserving ? sourceAudio.SampleRate : 44100;
        int? channelCount = sourcePreserving ? sourceAudio.ChannelCount : 2;
        MediaAudioChannelLayoutId layout = sourcePreserving
            ? sourceAudio.ChannelLayout
            : MediaAudioChannelLayoutId.Stereo;

        return new TargetMediaContract(
            presetId,
            mode,
            MediaContainerId.Mp3,
            expectsVideo: false,
            MediaCodecId.Unknown,
            MediaPixelFormatId.Unknown,
            videoBitDepth: null,
            MediaColorTransferId.Unknown,
            MediaHdrState.Unknown,
            expectsAudio: true,
            codec,
            sampleRate,
            channelCount,
            layout,
            hasChapters: false,
            hasGlobalMetadata: false,
            hasPolicyRelevantStreamMetadata: false);
    }
}
