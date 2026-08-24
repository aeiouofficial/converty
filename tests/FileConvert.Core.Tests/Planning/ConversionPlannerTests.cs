using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;
using FileConvert.Core.Capabilities;
using FileConvert.Core.Planning;

namespace FileConvert.Core.Tests.Planning;

public sealed class ConversionPlannerTests
{
    private static readonly FileFamilyId Audio = FileFamilyId.Parse("audio");
    private static readonly FormatId Wav = FormatId.Parse("audio.wav");
    private static readonly FormatId Mp3 = FormatId.Parse("audio.mp3");

    [Fact]
    public void Plan_selects_unique_highest_priority_provider()
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
    public void Plan_rejects_unsupported_route()
    {
        var planner = new ConversionPlanner(new CapabilityGraph([]));
        var source = new ProbedFileDescriptor("chapter.wav", Audio, Wav, 1234);
        Assert.Throws<ConversionPlanningException>(() => planner.Plan(new PlanningRequest(Guid.NewGuid(), source, Mp3)));
    }

    [Fact]
    public void Plan_rejects_identity_conversion_by_default()
    {
        var capability = new CapabilityDescriptor(ProviderId.Parse("provider.one"), Wav, Wav, ConversionMode.Copy, 100);
        var planner = new ConversionPlanner(new CapabilityGraph([capability]));
        var source = new ProbedFileDescriptor("chapter.wav", Audio, Wav, 1234);
        Assert.Throws<ConversionPlanningException>(() => planner.Plan(new PlanningRequest(Guid.NewGuid(), source, Wav)));
    }

    [Fact]
    public void Plan_rejects_top_priority_ambiguity()
    {
        var first = new CapabilityDescriptor(ProviderId.Parse("provider.first"), Wav, Mp3, ConversionMode.Transcode, 100);
        var second = new CapabilityDescriptor(ProviderId.Parse("provider.second"), Wav, Mp3, ConversionMode.Transcode, 100);
        var planner = new ConversionPlanner(new CapabilityGraph([first, second]));
        var source = new ProbedFileDescriptor("chapter.wav", Audio, Wav, 1234);
        Assert.Throws<ConversionPlanningException>(() => planner.Plan(new PlanningRequest(Guid.NewGuid(), source, Mp3)));
    }
}
