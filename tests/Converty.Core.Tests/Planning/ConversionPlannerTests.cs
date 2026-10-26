using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Capabilities;
using Converty.Core.Planning;

namespace Converty.Core.Tests.Planning;

public sealed class ConversionPlannerTests
{
    private static readonly FileFamilyId Audio = FileFamilyId.Parse("audio");
    private static readonly FileFamilyId VideoFamily = FileFamilyId.Parse("video");
    private static readonly FormatId Wav = FormatId.Parse("audio.wav");
    private static readonly FormatId Mp3 = FormatId.Parse("audio.mp3");
    private static readonly FormatId Mp4 = FormatId.Parse("video.mp4");
    private static readonly ProviderId Ffmpeg = ProviderId.Parse("provider.ffmpeg");
    private static readonly PresetId Mp4Preset = PresetId.Parse("video.mp4.h264");

    [Fact]
    public void PlanSelectsUniqueHighestPriorityProvider()
    {
        var preferred = new CapabilityDescriptor(ProviderId.Parse("provider.preferred"), Wav, Mp3, ConversionMode.Transcode, 100);
        var fallback = new CapabilityDescriptor(ProviderId.Parse("provider.fallback"), Wav, Mp3, ConversionMode.Transcode, 50);
        var planner = new ConversionPlanner(new CapabilityGraph([fallback, preferred]));
        var source = new ProbedFileDescriptor("chapter.wav", Audio, Wav, 1234);
        var plan = planner.Plan(new PlanningRequest(Guid.Parse("11111111-1111-1111-1111-111111111111"), source, Mp3));
        Assert.Equal(preferred.ProviderId, plan.ProviderId);
        Assert.Equal(ConversionMode.Transcode, plan.Mode);
        Assert.Equal(Mp3, plan.TargetFormat);
    }

    [Fact]
    public void PlanRejectsUnsupportedRoute()
    {
        var planner = new ConversionPlanner(new CapabilityGraph([]));
        var source = new ProbedFileDescriptor("chapter.wav", Audio, Wav, 1234);
        Assert.Throws<ConversionPlanningException>(() => planner.Plan(new PlanningRequest(Guid.NewGuid(), source, Mp3)));
    }

    [Fact]
    public void PlanRejectsIdentityConversionByDefault()
    {
        var capability = new CapabilityDescriptor(ProviderId.Parse("provider.one"), Wav, Wav, ConversionMode.Copy, 100);
        var planner = new ConversionPlanner(new CapabilityGraph([capability]));
        var source = new ProbedFileDescriptor("chapter.wav", Audio, Wav, 1234);
        Assert.Throws<ConversionPlanningException>(() => planner.Plan(new PlanningRequest(Guid.NewGuid(), source, Wav)));
    }

    [Fact]
    public void PlanRejectsTopPriorityAmbiguity()
    {
        var first = new CapabilityDescriptor(ProviderId.Parse("provider.first"), Wav, Mp3, ConversionMode.Transcode, 100);
        var second = new CapabilityDescriptor(ProviderId.Parse("provider.second"), Wav, Mp3, ConversionMode.Transcode, 100);
        var planner = new ConversionPlanner(new CapabilityGraph([first, second]));
        var source = new ProbedFileDescriptor("chapter.wav", Audio, Wav, 1234);
        Assert.Throws<ConversionPlanningException>(() => planner.Plan(new PlanningRequest(Guid.NewGuid(), source, Mp3)));
    }

    [Fact]
    public void PlanUsesVideoPolicyModeBeforeCapabilityAmbiguityResolution()
    {
        var planner = new ConversionPlanner(new CapabilityGraph(
        [
            new CapabilityDescriptor(Ffmpeg, Mp4, Mp4, ConversionMode.Copy, 100),
            new CapabilityDescriptor(Ffmpeg, Mp4, Mp4, ConversionMode.Remux, 100),
            new CapabilityDescriptor(Ffmpeg, Mp4, Mp4, ConversionMode.Transcode, 100),
        ]));
        var source = new ProbedFileDescriptor(
            "clip.mp4",
            VideoFamily,
            Mp4,
            1234,
            new MediaProbeFactsV1(
                MediaContainerId.Mp4,
                [VideoStream(MediaCodecId.H264), AudioStream(MediaCodecId.Aac)],
                MediaProbeCompleteness.Complete,
                hasChapters: false,
                hasGlobalMetadata: false,
                hasPolicyRelevantStreamMetadata: false));

        ConversionPlan plan = planner.Plan(new PlanningRequest(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            source,
            Mp4,
            presetId: Mp4Preset,
            allowIdentity: true));

        Assert.Equal(Ffmpeg, plan.ProviderId);
        Assert.Equal(ConversionMode.Copy, plan.Mode);
        Assert.Equal(Mp4Preset, plan.PresetId);
    }

    [Fact]
    public void PlanRejectsVideoRequestWhenRequiredProbeFactsAreMissing()
    {
        var planner = new ConversionPlanner(new CapabilityGraph(
        [
            new CapabilityDescriptor(Ffmpeg, Mp4, Mp4, ConversionMode.Copy, 100),
            new CapabilityDescriptor(Ffmpeg, Mp4, Mp4, ConversionMode.Remux, 100),
            new CapabilityDescriptor(Ffmpeg, Mp4, Mp4, ConversionMode.Transcode, 100),
        ]));
        var source = new ProbedFileDescriptor("clip.mp4", VideoFamily, Mp4, 1234);

        ConversionPlanningException error = Assert.Throws<ConversionPlanningException>(() => planner.Plan(new PlanningRequest(
            Guid.NewGuid(),
            source,
            Mp4,
            presetId: Mp4Preset,
            allowIdentity: true)));

        Assert.Contains(nameof(VideoPlanningReasonCode.MissingProbeFacts), error.Message, StringComparison.Ordinal);
    }

    private static MediaStreamFactsV1 VideoStream(MediaCodecId codec)
        => new(
            index: 0,
            MediaStreamKind.Video,
            codec,
            MediaProfileId.H264High,
            isDefault: true,
            isAttachedPicture: false,
            MediaPixelFormatId.Yuv420p,
            bitDepth: 8,
            width: 1920,
            height: 1080,
            MediaColorTransferId.Bt709,
            MediaHdrState.Sdr,
            sampleRate: null,
            channelCount: null,
            MediaAudioChannelLayoutId.Unknown,
            hasPolicyRelevantMetadata: false);

    private static MediaStreamFactsV1 AudioStream(MediaCodecId codec)
        => new(
            index: 1,
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
            sampleRate: 48000,
            channelCount: 2,
            MediaAudioChannelLayoutId.Stereo,
            hasPolicyRelevantMetadata: false);
}
