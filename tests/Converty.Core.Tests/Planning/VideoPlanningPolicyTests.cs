using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Planning;

namespace Converty.Core.Tests.Planning;

public sealed class VideoPlanningPolicyTests
{
    private static readonly PresetId Mp4Preset = PresetId.Parse("video.mp4.h264");
    private static readonly PresetId WebmPreset = PresetId.Parse("video.webm.vp9");
    private static readonly PresetId ExtractMp3Preset = PresetId.Parse("extract.audio.mp3");

    [Fact]
    public void Mp4TargetSelectsCopyForExactCompatibleMp4()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264), Audio(MediaCodecId.Aac, 48000, 2, MediaAudioChannelLayoutId.Stereo)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Copy, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.AlreadyTargetCompatible, decision.ReasonCode);
        Assert.Equal(Mp4Preset, decision.TargetContractId);
    }

    [Fact]
    public void Mp4TargetSelectsRemuxWhenOnlyContainerChanges()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            Mp4Preset,
            Facts(MediaContainerId.Matroska, Video(MediaCodecId.H264), Audio(MediaCodecId.Aac, 44100, 2, MediaAudioChannelLayoutId.Stereo)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Remux, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.ContainerChangeOnly, decision.ReasonCode);
    }

    [Fact]
    public void Mp4TargetSelectsTranscodeForQualifiedIncompatibleElementaryStreams()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            Mp4Preset,
            Facts(MediaContainerId.WebM, Video(MediaCodecId.Vp9), Audio(MediaCodecId.Opus, 48000, 2, MediaAudioChannelLayoutId.Stereo)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Transcode, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.TranscodeRequired, decision.ReasonCode);
    }

    [Fact]
    public void WebmTargetSelectsCopyForExactCompatibleWebm()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            WebmPreset,
            Facts(MediaContainerId.WebM, Video(MediaCodecId.Vp9), Audio(MediaCodecId.Opus, 48000, 2, MediaAudioChannelLayoutId.Stereo)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Copy, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.AlreadyTargetCompatible, decision.ReasonCode);
        Assert.Equal(WebmPreset, decision.TargetContractId);
    }

    [Fact]
    public void WebmTargetSelectsRemuxWhenOnlyContainerChanges()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            WebmPreset,
            Facts(MediaContainerId.Matroska, Video(MediaCodecId.Vp9), Audio(MediaCodecId.Opus, 48000, 2, MediaAudioChannelLayoutId.Stereo)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Remux, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.ContainerChangeOnly, decision.ReasonCode);
    }

    [Fact]
    public void WebmTargetSelectsTranscodeForQualifiedIncompatibleElementaryStreams()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            WebmPreset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264), Audio(MediaCodecId.Aac, 48000, 2, MediaAudioChannelLayoutId.Stereo)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Transcode, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.TranscodeRequired, decision.ReasonCode);
    }

    [Fact]
    public void ExtractMp3SelectsRemuxForCompatibleMp3Audio()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            ExtractMp3Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264), Audio(MediaCodecId.Mp3, 44100, 2, MediaAudioChannelLayoutId.Stereo)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Remux, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.AudioExtractionStreamCopy, decision.ReasonCode);
    }

    [Fact]
    public void ExtractMp3SelectsTranscodeForQualifiedAudioCodec()
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(
            ExtractMp3Preset,
            Facts(MediaContainerId.Mov, Video(MediaCodecId.H264), Audio(MediaCodecId.Aac, 48000, 6, MediaAudioChannelLayoutId.Multichannel)));

        Assert.True(decision.IsAllowed);
        Assert.Equal(ConversionMode.Transcode, decision.Mode);
        Assert.Equal(VideoPlanningReasonCode.AudioExtractionTranscode, decision.ReasonCode);
    }

    [Fact]
    public void RejectsUnknownContainer()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Unknown, Video(MediaCodecId.H264)),
            VideoPlanningReasonCode.UnknownContainer);
    }

    [Fact]
    public void RejectsUnsupportedContainer()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp3, Video(MediaCodecId.H264)),
            VideoPlanningReasonCode.UnsupportedContainer);
    }

    [Fact]
    public void RejectsUnknownCodec()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.Unknown)),
            VideoPlanningReasonCode.UnknownCodec);
    }

    [Fact]
    public void RejectsCodecOutsideQualifiedDecoderAllowlist()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.OtherKnown)),
            VideoPlanningReasonCode.CodecNotQualifiedForDecode);
    }

    [Fact]
    public void RejectsIncompleteProbeFacts()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, MediaProbeCompleteness.Incomplete, Video(MediaCodecId.H264)),
            VideoPlanningReasonCode.IncompleteProbeFacts);
    }

    [Fact]
    public void RejectsMissingRequiredVideoFact()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264, bitDepth: null)),
            VideoPlanningReasonCode.MissingRequiredVideoFact);
    }

    [Fact]
    public void RejectsMultiplePrimaryVideoStreams()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264, index: 0), Video(MediaCodecId.H264, index: 1)),
            VideoPlanningReasonCode.MultipleVideoStreams);
    }

    [Fact]
    public void RejectsMultiplePrimaryAudioStreams()
    {
        AssertRejected(
            Mp4Preset,
            Facts(
                MediaContainerId.Mp4,
                Video(MediaCodecId.H264),
                Audio(MediaCodecId.Aac, 48000, 2, MediaAudioChannelLayoutId.Stereo, index: 1),
                Audio(MediaCodecId.Aac, 48000, 2, MediaAudioChannelLayoutId.Stereo, index: 2)),
            VideoPlanningReasonCode.MultipleAudioStreams);
    }

    [Theory]
    [InlineData(MediaStreamKind.Subtitle, VideoPlanningReasonCode.SubtitleStreamUnsupported)]
    [InlineData(MediaStreamKind.Data, VideoPlanningReasonCode.DataStreamUnsupported)]
    [InlineData(MediaStreamKind.Attachment, VideoPlanningReasonCode.AttachmentStreamUnsupported)]
    public void RejectsPolicySensitiveExtraStreams(MediaStreamKind kind, VideoPlanningReasonCode expectedReason)
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264), Other(kind, 1)),
            expectedReason);
    }

    [Fact]
    public void RejectsHdrInputWithoutQualification()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264, hdrState: MediaHdrState.Hdr, colorTransfer: MediaColorTransferId.Smpte2084)),
            VideoPlanningReasonCode.HdrUnsupported);
    }

    [Fact]
    public void RejectsHighBitDepthInputWithoutQualification()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264, bitDepth: 10)),
            VideoPlanningReasonCode.HighBitDepthUnsupported);
    }

    [Fact]
    public void RejectsMissingAudioForExtraction()
    {
        AssertRejected(
            ExtractMp3Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264)),
            VideoPlanningReasonCode.MissingAudioStream);
    }

    [Fact]
    public void RejectsMultipleAudioStreamsForExtraction()
    {
        AssertRejected(
            ExtractMp3Preset,
            Facts(
                MediaContainerId.Mp4,
                Video(MediaCodecId.H264),
                Audio(MediaCodecId.Mp3, 44100, 2, MediaAudioChannelLayoutId.Stereo, index: 1),
                Audio(MediaCodecId.Aac, 48000, 2, MediaAudioChannelLayoutId.Stereo, index: 2)),
            VideoPlanningReasonCode.MultipleAudioStreams);
    }

    [Fact]
    public void RejectsMissingRequiredAudioFact()
    {
        AssertRejected(
            Mp4Preset,
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264), Audio(MediaCodecId.Aac, null, 2, MediaAudioChannelLayoutId.Stereo)),
            VideoPlanningReasonCode.MissingRequiredAudioFact);
    }

    [Fact]
    public void RejectsUnsupportedPreset()
    {
        AssertRejected(
            PresetId.Parse("audio.mp3"),
            Facts(MediaContainerId.Mp4, Video(MediaCodecId.H264), Audio(MediaCodecId.Aac, 48000, 2, MediaAudioChannelLayoutId.Stereo)),
            VideoPlanningReasonCode.UnsupportedPreset);
    }

    private static void AssertRejected(PresetId presetId, MediaProbeFactsV1 facts, VideoPlanningReasonCode expectedReason)
    {
        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(presetId, facts);
        Assert.False(decision.IsAllowed);
        Assert.Null(decision.Mode);
        Assert.Equal(expectedReason, decision.ReasonCode);
        Assert.Equal(presetId, decision.TargetContractId);
    }

    private static MediaProbeFactsV1 Facts(MediaContainerId container, params MediaStreamFactsV1[] streams)
        => Facts(container, MediaProbeCompleteness.Complete, streams);

    private static MediaProbeFactsV1 Facts(
        MediaContainerId container,
        MediaProbeCompleteness completeness,
        params MediaStreamFactsV1[] streams)
        => new(container, streams, completeness, hasChapters: false, hasGlobalMetadata: false, hasPolicyRelevantStreamMetadata: false);

    private static MediaStreamFactsV1 Video(
        MediaCodecId codec,
        int index = 0,
        int? bitDepth = 8,
        MediaHdrState hdrState = MediaHdrState.Sdr,
        MediaColorTransferId colorTransfer = MediaColorTransferId.Bt709,
        MediaPixelFormatId pixelFormat = MediaPixelFormatId.Yuv420p)
        => new(
            index,
            MediaStreamKind.Video,
            codec,
            MediaProfileId.Unknown,
            isDefault: true,
            isAttachedPicture: false,
            pixelFormat,
            bitDepth,
            width: 1920,
            height: 1080,
            colorTransfer,
            hdrState,
            sampleRate: null,
            channelCount: null,
            MediaAudioChannelLayoutId.Unknown,
            hasPolicyRelevantMetadata: false);

    private static MediaStreamFactsV1 Audio(
        MediaCodecId codec,
        int? sampleRate,
        int? channelCount,
        MediaAudioChannelLayoutId channelLayout,
        int index = 1)
        => new(
            index,
            MediaStreamKind.Audio,
            codec,
            MediaProfileId.Unknown,
            isDefault: true,
            isAttachedPicture: false,
            MediaPixelFormatId.Unknown,
            bitDepth: null,
            width: null,
            height: null,
            MediaColorTransferId.Unknown,
            MediaHdrState.Unknown,
            sampleRate,
            channelCount,
            channelLayout,
            hasPolicyRelevantMetadata: false);

    private static MediaStreamFactsV1 Other(MediaStreamKind kind, int index)
        => new(
            index,
            kind,
            MediaCodecId.OtherKnown,
            MediaProfileId.Unknown,
            isDefault: false,
            isAttachedPicture: false,
            MediaPixelFormatId.Unknown,
            bitDepth: null,
            width: null,
            height: null,
            MediaColorTransferId.Unknown,
            MediaHdrState.Unknown,
            sampleRate: null,
            channelCount: null,
            MediaAudioChannelLayoutId.Unknown,
            hasPolicyRelevantMetadata: false);
}
