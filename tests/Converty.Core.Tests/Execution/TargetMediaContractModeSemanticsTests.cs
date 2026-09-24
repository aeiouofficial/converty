using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Execution;

namespace Converty.Core.Tests.Execution;

public sealed class TargetMediaContractModeSemanticsTests
{
    [Fact]
    public void CopyPreservesSourceTechnicalFactsAndMetadata()
    {
        MediaProbeFactsV1 source = CreateSource(MediaContainerId.Mp4, metadata: true);
        TargetMediaContract contract = TargetMediaContract.ForPlan(
            PresetId.Parse("video.mp4.h264"), ConversionMode.Copy, source);

        Assert.Equal(MediaContainerId.Mp4, contract.Container);
        Assert.Equal(MediaColorTransferId.OtherKnown, contract.VideoColorTransfer);
        Assert.Equal(MediaHdrState.Sdr, contract.VideoHdrState);
        Assert.Equal(44100, contract.AudioSampleRate);
        Assert.Equal(1, contract.AudioChannelCount);
        Assert.Equal(MediaAudioChannelLayoutId.Mono, contract.AudioChannelLayout);
        Assert.True(contract.HasChapters);
        Assert.True(contract.HasGlobalMetadata);
        Assert.True(contract.HasPolicyRelevantStreamMetadata);
    }

    [Fact]
    public void RemuxPreservesSourceTechnicalFactsAndStripsMetadata()
    {
        MediaProbeFactsV1 source = CreateSource(MediaContainerId.Mov, metadata: true);
        TargetMediaContract contract = TargetMediaContract.ForPlan(
            PresetId.Parse("video.mp4.h264"), ConversionMode.Remux, source);

        Assert.Equal(MediaContainerId.Mp4, contract.Container);
        Assert.Equal(MediaColorTransferId.OtherKnown, contract.VideoColorTransfer);
        Assert.Equal(MediaHdrState.Sdr, contract.VideoHdrState);
        Assert.Equal(44100, contract.AudioSampleRate);
        Assert.Equal(1, contract.AudioChannelCount);
        Assert.Equal(MediaAudioChannelLayoutId.Mono, contract.AudioChannelLayout);
        Assert.False(contract.HasChapters);
        Assert.False(contract.HasGlobalMetadata);
        Assert.False(contract.HasPolicyRelevantStreamMetadata);
    }

    [Fact]
    public void TranscodeUsesFixedCompatibilityFacts()
    {
        MediaProbeFactsV1 source = CreateSource(MediaContainerId.Avi, metadata: true);
        TargetMediaContract contract = TargetMediaContract.ForPlan(
            PresetId.Parse("video.mp4.h264"), ConversionMode.Transcode, source);

        Assert.Equal(MediaContainerId.Mp4, contract.Container);
        Assert.Equal(MediaCodecId.H264, contract.VideoCodec);
        Assert.Equal(MediaPixelFormatId.Yuv420p, contract.VideoPixelFormat);
        Assert.Equal(8, contract.VideoBitDepth);
        Assert.Equal(MediaColorTransferId.Bt709, contract.VideoColorTransfer);
        Assert.Equal(MediaHdrState.Sdr, contract.VideoHdrState);
        Assert.Equal(MediaCodecId.Aac, contract.AudioCodec);
        Assert.Equal(48000, contract.AudioSampleRate);
        Assert.Equal(2, contract.AudioChannelCount);
        Assert.Equal(MediaAudioChannelLayoutId.Stereo, contract.AudioChannelLayout);
        Assert.False(contract.HasChapters);
        Assert.False(contract.HasGlobalMetadata);
        Assert.False(contract.HasPolicyRelevantStreamMetadata);
    }

    private static MediaProbeFactsV1 CreateSource(MediaContainerId container, bool metadata) =>
        new(
            container,
            [
                new MediaStreamFactsV1(
                    0, MediaStreamKind.Video, MediaCodecId.H264, MediaProfileId.H264High,
                    true, false, MediaPixelFormatId.Yuv420p, 8, 1920, 1080,
                    MediaColorTransferId.OtherKnown, MediaHdrState.Sdr,
                    null, null, MediaAudioChannelLayoutId.Unknown, false),
                new MediaStreamFactsV1(
                    1, MediaStreamKind.Audio, MediaCodecId.Aac, MediaProfileId.Unknown,
                    true, false, MediaPixelFormatId.Unknown, null, null, null,
                    MediaColorTransferId.Unknown, MediaHdrState.Unknown,
                    44100, 1, MediaAudioChannelLayoutId.Mono, false),
            ],
            MediaProbeCompleteness.Complete,
            metadata,
            metadata,
            metadata);
}
