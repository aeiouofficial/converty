using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Planning;

public static class VideoPlanningPolicy
{
    private static readonly HashSet<MediaContainerId> SupportedVideoContainers =
    [
        MediaContainerId.Mp4,
        MediaContainerId.Mov,
        MediaContainerId.Matroska,
        MediaContainerId.Avi,
        MediaContainerId.WebM,
        MediaContainerId.Mpeg,
        MediaContainerId.Wmv,
    ];

    private static readonly HashSet<MediaCodecId> QualifiedVideoDecoders =
    [
        MediaCodecId.H264,
        MediaCodecId.Vp9,
        MediaCodecId.Mpeg4,
        MediaCodecId.Mpeg2Video,
        MediaCodecId.Wmv2,
    ];

    private static readonly HashSet<MediaCodecId> QualifiedAudioDecoders =
    [
        MediaCodecId.Aac,
        MediaCodecId.Opus,
        MediaCodecId.Mp3,
        MediaCodecId.Mp2,
        MediaCodecId.Wmav2,
    ];

    public static bool IsVideoPreset(PresetId presetId)
    {
        ArgumentNullException.ThrowIfNull(presetId);
        return presetId.Value is "video.mp4.h264" or "video.webm.vp9" or "extract.audio.mp3";
    }

    public static VideoExecutionDecision Evaluate(PresetId presetId, MediaProbeFactsV1 facts)
    {
        ArgumentNullException.ThrowIfNull(presetId);
        ArgumentNullException.ThrowIfNull(facts);

        if (!IsVideoPreset(presetId))
        {
            return Reject(presetId, VideoPlanningReasonCode.UnsupportedPreset);
        }

        VideoExecutionDecision? commonRejection = ValidateCommonFacts(presetId, facts);
        if (commonRejection is not null)
        {
            return commonRejection;
        }

        return presetId.Value switch
        {
            "video.mp4.h264" => EvaluateVideoTarget(presetId, facts, MediaContainerId.Mp4, MediaCodecId.H264, MediaCodecId.Aac),
            "video.webm.vp9" => EvaluateVideoTarget(presetId, facts, MediaContainerId.WebM, MediaCodecId.Vp9, MediaCodecId.Opus),
            "extract.audio.mp3" => EvaluateAudioExtraction(presetId, facts),
            _ => Reject(presetId, VideoPlanningReasonCode.UnsupportedPreset),
        };
    }

    private static VideoExecutionDecision? ValidateCommonFacts(PresetId presetId, MediaProbeFactsV1 facts)
    {
        if (facts.Completeness != MediaProbeCompleteness.Complete)
        {
            return Reject(presetId, VideoPlanningReasonCode.IncompleteProbeFacts);
        }

        if (facts.Container == MediaContainerId.Unknown)
        {
            return Reject(presetId, VideoPlanningReasonCode.UnknownContainer);
        }

        if (!SupportedVideoContainers.Contains(facts.Container))
        {
            return Reject(presetId, VideoPlanningReasonCode.UnsupportedContainer);
        }

        return null;
    }

    private static VideoExecutionDecision EvaluateVideoTarget(
        PresetId presetId,
        MediaProbeFactsV1 facts,
        MediaContainerId targetContainer,
        MediaCodecId passthroughVideoCodec,
        MediaCodecId passthroughAudioCodec)
    {
        foreach (MediaStreamFactsV1 stream in facts.Streams)
        {
            VideoExecutionDecision? streamKindRejection = stream.Kind switch
            {
                MediaStreamKind.Subtitle => Reject(presetId, VideoPlanningReasonCode.SubtitleStreamUnsupported),
                MediaStreamKind.Data => Reject(presetId, VideoPlanningReasonCode.DataStreamUnsupported),
                MediaStreamKind.Attachment => Reject(presetId, VideoPlanningReasonCode.AttachmentStreamUnsupported),
                MediaStreamKind.Unknown => Reject(presetId, VideoPlanningReasonCode.UnknownStreamKind),
                _ => null,
            };
            if (streamKindRejection is not null)
            {
                return streamKindRejection;
            }
        }

        MediaStreamFactsV1[] videoStreams = facts.Streams
            .Where(stream => stream.Kind == MediaStreamKind.Video && !stream.IsAttachedPicture)
            .ToArray();
        int allVideoStreamCount = facts.Streams.Count(stream => stream.Kind == MediaStreamKind.Video);
        if (videoStreams.Length == 0)
        {
            return Reject(presetId, VideoPlanningReasonCode.MissingVideoStream);
        }

        if (videoStreams.Length != 1 || allVideoStreamCount != 1)
        {
            return Reject(presetId, VideoPlanningReasonCode.MultipleVideoStreams);
        }

        MediaStreamFactsV1[] audioStreams = facts.Streams.Where(stream => stream.Kind == MediaStreamKind.Audio).ToArray();
        if (audioStreams.Length > 1)
        {
            return Reject(presetId, VideoPlanningReasonCode.MultipleAudioStreams);
        }

        MediaStreamFactsV1 video = videoStreams[0];
        VideoExecutionDecision? videoRejection = ValidateVideoStream(presetId, video);
        if (videoRejection is not null)
        {
            return videoRejection;
        }

        MediaStreamFactsV1? audio = audioStreams.SingleOrDefault();
        if (audio is not null)
        {
            VideoExecutionDecision? audioRejection = ValidateAudioStream(presetId, audio);
            if (audioRejection is not null)
            {
                return audioRejection;
            }
        }

        bool passthroughAudio = audio is null || IsPassthroughAudioCompatible(presetId, audio, passthroughAudioCodec);
        bool passthroughVideo = video.Codec == passthroughVideoCodec
            && video.PixelFormat == MediaPixelFormatId.Yuv420p
            && video.BitDepth == 8;

        if (passthroughVideo && passthroughAudio)
        {
            return facts.Container == targetContainer
                ? Allow(presetId, ConversionMode.Copy, VideoPlanningReasonCode.AlreadyTargetCompatible)
                : Allow(presetId, ConversionMode.Remux, VideoPlanningReasonCode.ContainerChangeOnly);
        }

        return Allow(presetId, ConversionMode.Transcode, VideoPlanningReasonCode.TranscodeRequired);
    }

    private static VideoExecutionDecision EvaluateAudioExtraction(PresetId presetId, MediaProbeFactsV1 facts)
    {
        MediaStreamFactsV1[] audioStreams = facts.Streams.Where(stream => stream.Kind == MediaStreamKind.Audio).ToArray();
        if (audioStreams.Length == 0)
        {
            return Reject(presetId, VideoPlanningReasonCode.MissingAudioStream);
        }

        if (audioStreams.Length > 1)
        {
            return Reject(presetId, VideoPlanningReasonCode.MultipleAudioStreams);
        }

        MediaStreamFactsV1 audio = audioStreams[0];
        VideoExecutionDecision? audioRejection = ValidateAudioStream(presetId, audio);
        if (audioRejection is not null)
        {
            return audioRejection;
        }

        bool streamCopyCompatible = audio.Codec == MediaCodecId.Mp3
            && audio.ChannelCount is >= 1 and <= 2
            && audio.SampleRate is 32000 or 44100 or 48000
            && audio.ChannelLayout is MediaAudioChannelLayoutId.Mono or MediaAudioChannelLayoutId.Stereo;

        return streamCopyCompatible
            ? Allow(presetId, ConversionMode.Remux, VideoPlanningReasonCode.AudioExtractionStreamCopy)
            : Allow(presetId, ConversionMode.Transcode, VideoPlanningReasonCode.AudioExtractionTranscode);
    }

    private static VideoExecutionDecision? ValidateVideoStream(PresetId presetId, MediaStreamFactsV1 stream)
    {
        if (stream.Codec == MediaCodecId.Unknown)
        {
            return Reject(presetId, VideoPlanningReasonCode.UnknownCodec);
        }

        if (!QualifiedVideoDecoders.Contains(stream.Codec))
        {
            return Reject(presetId, VideoPlanningReasonCode.CodecNotQualifiedForDecode);
        }

        if (stream.PixelFormat == MediaPixelFormatId.Unknown
            || stream.BitDepth is null
            || stream.Width is null
            || stream.Height is null
            || stream.ColorTransfer == MediaColorTransferId.Unknown
            || stream.HdrState == MediaHdrState.Unknown)
        {
            return Reject(presetId, VideoPlanningReasonCode.MissingRequiredVideoFact);
        }

        if (stream.HdrState == MediaHdrState.Hdr
            || stream.ColorTransfer is MediaColorTransferId.Smpte2084 or MediaColorTransferId.AribStdB67)
        {
            return Reject(presetId, VideoPlanningReasonCode.HdrUnsupported);
        }

        if (stream.BitDepth != 8)
        {
            return Reject(presetId, VideoPlanningReasonCode.HighBitDepthUnsupported);
        }

        if (stream.PixelFormat != MediaPixelFormatId.Yuv420p)
        {
            return Reject(presetId, VideoPlanningReasonCode.UnsupportedPixelFormat);
        }

        if (stream.ColorTransfer != MediaColorTransferId.Bt709)
        {
            return Reject(presetId, VideoPlanningReasonCode.UnsupportedColorTransfer);
        }

        return null;
    }

    private static VideoExecutionDecision? ValidateAudioStream(PresetId presetId, MediaStreamFactsV1 stream)
    {
        if (stream.Codec == MediaCodecId.Unknown)
        {
            return Reject(presetId, VideoPlanningReasonCode.UnknownCodec);
        }

        if (!QualifiedAudioDecoders.Contains(stream.Codec))
        {
            return Reject(presetId, VideoPlanningReasonCode.CodecNotQualifiedForDecode);
        }

        if (stream.SampleRate is null
            || stream.ChannelCount is null
            || stream.ChannelLayout == MediaAudioChannelLayoutId.Unknown
            || stream.ChannelLayout == MediaAudioChannelLayoutId.OtherKnown)
        {
            return Reject(presetId, VideoPlanningReasonCode.MissingRequiredAudioFact);
        }

        return null;
    }

    private static bool IsPassthroughAudioCompatible(
        PresetId presetId,
        MediaStreamFactsV1 audio,
        MediaCodecId passthroughAudioCodec)
    {
        if (audio.Codec != passthroughAudioCodec || audio.ChannelCount is not (>= 1 and <= 2))
        {
            return false;
        }

        return presetId.Value switch
        {
            "video.mp4.h264" => audio.SampleRate is 44100 or 48000
                && audio.ChannelLayout is MediaAudioChannelLayoutId.Mono or MediaAudioChannelLayoutId.Stereo,
            "video.webm.vp9" => audio.SampleRate == 48000
                && audio.ChannelLayout is MediaAudioChannelLayoutId.Mono or MediaAudioChannelLayoutId.Stereo,
            _ => false,
        };
    }

    private static VideoExecutionDecision Allow(
        PresetId presetId,
        ConversionMode mode,
        VideoPlanningReasonCode reasonCode)
        => VideoExecutionDecision.Allow(mode, reasonCode, presetId);

    private static VideoExecutionDecision Reject(PresetId presetId, VideoPlanningReasonCode reasonCode)
        => VideoExecutionDecision.Reject(reasonCode, presetId);
}
