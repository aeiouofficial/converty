using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Contracts.Jobs;

namespace Converty.Contracts.Tests.Conversion;

public sealed class ContractTests
{
    [Fact]
    public void SchemaVersionsCurrentIsOne() => Assert.Equal(1, SchemaVersions.Current);

    [Fact]
    public void ConversionRequestRequiresAtLeastOneFile() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertUsingDefault, Array.Empty<string>(), null, null));

    [Fact]
    public void ConversionRequestRejectsUnknownSchemaVersion() => Assert.Throws<ArgumentOutOfRangeException>(() => new ConversionRequest(99, Guid.NewGuid(), ConversionAction.ConvertUsingDefault, ["chapter.wav"], null, null));

    [Fact]
    public void ConversionRequestRejectsUndefinedActionValue() => Assert.Throws<ArgumentOutOfRangeException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), (ConversionAction)999, ["chapter.wav"], null, null));

    [Fact]
    public void ConvertToFormatRequiresTargetFormat() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertToFormat, ["chapter.wav"], null, null));

    [Fact]
    public void ConvertWithPresetRejectsRedundantTargetFormat() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertWithPreset, ["chapter.wav"], FormatId.Parse("audio.mp3"), PresetId.Parse("audio.mp3.high")));

    [Fact]
    public void ConvertWithPresetRequiresPresetId() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertWithPreset, ["chapter.wav"], FormatId.Parse("audio.mp3"), null));

    [Fact]
    public void JobStatusSnapshotRejectsUndefinedState() => Assert.Throws<ArgumentOutOfRangeException>(() => new JobStatusSnapshot(SchemaVersions.Current, Guid.NewGuid(), Guid.NewGuid(), (ConversionJobState)999, null, null));

    [Fact]
    public void JobStatusSnapshotRejectsProgressOutsideZeroToOne() => Assert.Throws<ArgumentOutOfRangeException>(() => new JobStatusSnapshot(SchemaVersions.Current, Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Converting, 1.1, null));

    [Fact]
    public void CapabilityDescriptorRejectsUndefinedConversionMode() => Assert.Throws<ArgumentOutOfRangeException>(() => new CapabilityDescriptor(ProviderId.Parse("fake.audio"), FormatId.Parse("audio.wav"), FormatId.Parse("audio.mp3"), (ConversionMode)999, 100));

    [Fact]
    public void ConversionPlanRejectsUndefinedConversionMode()
    {
        var source = new ProbedFileDescriptor("chapter.wav", FileFamilyId.Parse("audio"), FormatId.Parse("audio.wav"), 128);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConversionPlan(SchemaVersions.Current, Guid.NewGuid(), source, FormatId.Parse("audio.mp3"), ProviderId.Parse("fake.audio"), (ConversionMode)999, null));
    }
}
