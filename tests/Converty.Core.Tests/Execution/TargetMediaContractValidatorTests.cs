using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Execution;

namespace Converty.Core.Tests.Execution;

public sealed class TargetMediaContractValidatorTests
{
    [Fact]
    public void ValidateAcceptsExactMp4TranscodeContract()
    {
        MediaProbeFactsV1 source = CreateFacts(
            container: MediaContainerId.Avi,
            videoCodec: MediaCodecId.Mpeg4,
            audioCodec: MediaCodecId.Mp2,
            audioSampleRate: 44100,
            audioChannels: 2,
            audioLayout: MediaAudioChannelLayoutId.Stereo);
        TargetMediaContract contract = TargetMediaContract.ForPlan(
            PresetId.Parse("video.mp4.h264"),
            ConversionMode.Transcode,
            source);

        TargetMediaContractValidator.Validate(contract, CreateValidMp4TranscodeOutput());
    }

    [Theory]
    [InlineData("container")]
    [InlineData("video-codec")]
    [InlineData("pixel-format")]
    [InlineData("audio-rate")]
    [InlineData("audio-layout")]
    [InlineData("hdr")]
    [InlineData("secondary-audio")]
    public void ValidateRejectsOutputThatViolatesPromisedMp4Contract(string violation)
    {
        MediaProbeFactsV1 source = CreateFacts(
            container: MediaContainerId.Avi,
            videoCodec: MediaCodecId.Mpeg4,
            audioCodec: MediaCodecId.Mp2,
            audioSampleRate: 44100,
            audioChannels: 2,
            audioLayout: MediaAudioChannelLayoutId.Stereo);
        TargetMediaContract contract = TargetMediaContract.ForPlan(
            PresetId.Parse("video.mp4.h264"),
            ConversionMode.Transcode,
            source);

        MediaProbeFactsV1 output = violation switch
        {
            "container" => CreateValidMp4TranscodeOutput(container: MediaContainerId.WebM),
            "video-codec" => CreateValidMp4TranscodeOutput(videoCodec: MediaCodecId.Vp9),
            "pixel-format" => CreateValidMp4TranscodeOutput(pixelFormat: MediaPixelFormatId.OtherKnown),
            "audio-rate" => CreateValidMp4TranscodeOutput(audioSampleRate: 44100),
            "audio-layout" => CreateValidMp4TranscodeOutput(
                audioChannels: 1,
                audioLayout: MediaAudioChannelLayoutId.Mono),
            "hdr" => CreateValidMp4TranscodeOutput(
                transfer: MediaColorTransferId.Smpte2084,
                hdrState: MediaHdrState.Hdr),
            "secondary-audio" => CreateValidMp4TranscodeOutput(includeSecondaryAudio: true),
            _ => throw new InvalidOperationException("Unknown test violation."),
        };

        Assert.Throws<InvalidOperationException>(() => TargetMediaContractValidator.Validate(contract, output));
    }

    private static MediaProbeFactsV1 CreateValidMp4TranscodeOutput(
        MediaContainerId container = MediaContainerId.Mp4,
        MediaCodecId videoCodec = MediaCodecId.H264,
        MediaPixelFormatId pixelFormat = MediaPixelFormatId.Yuv420p,
        int audioSampleRate = 48000,
        int audioChannels = 2,
        MediaAudioChannelLayoutId audioLayout = MediaAudioChannelLayoutId.Stereo,
        MediaColorTransferId transfer = MediaColorTransferId.Bt709,
        MediaHdrState hdrState = MediaHdrState.Sdr,
        bool includeSecondaryAudio = false)
    {
        List<MediaStreamFactsV1> streams =
        [
            CreateVideoStream(videoCodec, pixelFormat, transfer, hdrState),
            CreateAudioStream(1, MediaCodecId.Aac, audioSampleRate, audioChannels, audioLayout),
        ];
        if (includeSecondaryAudio)
        {
            streams.Add(CreateAudioStream(2, MediaCodecId.Aac, 48000, 2, MediaAudioChannelLayoutId.Stereo));
        }

        return new MediaProbeFactsV1(
            container,
            streams,
            MediaProbeCompleteness.Complete,
            hasChapters: false,
            hasGlobalMetadata: false,
            hasPolicyRelevantStreamMetadata: false);
    }

    private static MediaProbeFactsV1 CreateFacts(
        MediaContainerId container,
        MediaCodecId videoCodec,
        MediaCodecId audioCodec,
        int audioSampleRate,
        int audioChannels,
        MediaAudioChannelLayoutId audioLayout) =>
        new(
            container,
            [
                CreateVideoStream(videoCodec, MediaPixelFormatId.Yuv420p, MediaColorTransferId.Bt709, MediaHdrState.Sdr),
                CreateAudioStream(1, audioCodec, audioSampleRate, audioChannels, audioLayout),
            ],
            MediaProbeCompleteness.Complete,
            hasChapters: false,
            hasGlobalMetadata: false,
            hasPolicyRelevantStreamMetadata: false);

    private static MediaStreamFactsV1 CreateVideoStream(
        MediaCodecId codec,
        MediaPixelFormatId pixelFormat,
        MediaColorTransferId transfer,
        MediaHdrState hdrState) =>
        new(
            index: 0,
            kind: MediaStreamKind.Video,
            codec: codec,
            profile: codec == MediaCodecId.H264 ? MediaProfileId.H264High : MediaProfileId.OtherKnown,
            isDefault: true,
            isAttachedPicture: false,
            pixelFormat: pixelFormat,
            bitDepth: 8,
            width: 1920,
            height: 1080,
            colorTransfer: transfer,
            hdrState: hdrState,
            sampleRate: null,
            channelCount: null,
            channelLayout: MediaAudioChannelLayoutId.Unknown,
            hasPolicyRelevantMetadata: false);

    private static MediaStreamFactsV1 CreateAudioStream(
        int index,
        MediaCodecId codec,
        int sampleRate,
        int channels,
        MediaAudioChannelLayoutId layout) =>
        new(
            index: index,
            kind: MediaStreamKind.Audio,
            codec: codec,
            profile: MediaProfileId.Unknown,
            isDefault: index == 1,
            isAttachedPicture: false,
            pixelFormat: MediaPixelFormatId.Unknown,
            bitDepth: null,
            width: null,
            height: null,
            colorTransfer: MediaColorTransferId.Unknown,
            hdrState: MediaHdrState.Unknown,
            sampleRate: sampleRate,
            channelCount: channels,
            channelLayout: layout,
            hasPolicyRelevantMetadata: false);
}
