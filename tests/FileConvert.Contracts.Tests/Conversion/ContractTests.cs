using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;
using FileConvert.Contracts.Jobs;

namespace FileConvert.Contracts.Tests.Conversion;

public sealed class ContractTests
{
    [Fact]
    public void SchemaVersions_current_is_one() => Assert.Equal(1, SchemaVersions.Current);

    [Fact]
    public void ConversionRequest_requires_at_least_one_file() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertUsingDefault, Array.Empty<string>(), null, null));

    [Fact]
    public void ConversionRequest_rejects_unknown_schema_version() => Assert.Throws<ArgumentOutOfRangeException>(() => new ConversionRequest(99, Guid.NewGuid(), ConversionAction.ConvertUsingDefault, ["chapter.wav"], null, null));

    [Fact]
    public void ConversionRequest_rejects_undefined_action_value() => Assert.Throws<ArgumentOutOfRangeException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), (ConversionAction)999, ["chapter.wav"], null, null));

    [Fact]
    public void ConvertToFormat_requires_target_format() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertToFormat, ["chapter.wav"], null, null));

    [Fact]
    public void ConvertWithPreset_rejects_redundant_target_format() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertWithPreset, ["chapter.wav"], FormatId.Parse("audio.mp3"), PresetId.Parse("audio.mp3.high")));

    [Fact]
    public void ConvertWithPreset_requires_preset_id() => Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertWithPreset, ["chapter.wav"], FormatId.Parse("audio.mp3"), null));

    [Fact]
    public void JobStatusSnapshot_rejects_undefined_state() => Assert.Throws<ArgumentOutOfRangeException>(() => new JobStatusSnapshot(SchemaVersions.Current, Guid.NewGuid(), Guid.NewGuid(), (ConversionJobState)999, null, null));

    [Fact]
    public void JobStatusSnapshot_rejects_progress_outside_zero_to_one() => Assert.Throws<ArgumentOutOfRangeException>(() => new JobStatusSnapshot(SchemaVersions.Current, Guid.NewGuid(), Guid.NewGuid(), ConversionJobState.Converting, 1.1, null));

    [Fact]
    public void CapabilityDescriptor_rejects_undefined_conversion_mode() => Assert.Throws<ArgumentOutOfRangeException>(() => new CapabilityDescriptor(ProviderId.Parse("fake.audio"), FormatId.Parse("audio.wav"), FormatId.Parse("audio.mp3"), (ConversionMode)999, 100));

    [Fact]
    public void ConversionPlan_rejects_undefined_conversion_mode()
    {
        var source = new ProbedFileDescriptor("chapter.wav", FileFamilyId.Parse("audio"), FormatId.Parse("audio.wav"), 128);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConversionPlan(SchemaVersions.Current, Guid.NewGuid(), source, FormatId.Parse("audio.mp3"), ProviderId.Parse("fake.audio"), (ConversionMode)999, null));
    }
}
