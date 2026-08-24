using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Contracts.Tests.Conversion;

public sealed class ValidationBoundaryTests
{
    [Fact]
    public void ConversionRequestRejectsPathLongerThanWireLimit()
    {
        var path = new string('a', ConversionRequest.MaximumPathLength + 1);
        Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertUsingDefault, [path], null, null));
    }

    [Fact]
    public void ConversionPresetRejectsTooManyOptions()
    {
        var options = Enumerable.Range(0, ConversionPreset.MaximumOptions + 1).ToDictionary(index => $"key{index}", _ => "value", StringComparer.Ordinal);
        Assert.Throws<ArgumentException>(() => new ConversionPreset(SchemaVersions.Current, PresetId.Parse("audio.mp3.high"), "High", FileFamilyId.Parse("audio"), FormatId.Parse("audio.mp3"), null, options));
    }

    [Fact]
    public void FormatDescriptorRejectsTooManyExtensions()
    {
        var extensions = Enumerable.Range(0, FormatDescriptor.MaximumExtensions + 1).Select(index => $".x{index}").ToArray();
        Assert.Throws<ArgumentException>(() => new FormatDescriptor(FormatId.Parse("image.test"), FileFamilyId.Parse("image"), "Test", extensions[0], extensions, SchemaVersions.Current));
    }

    [Fact]
    public void RootDescriptorsExposeCurrentSchemaVersion()
    {
        var format = new FormatDescriptor(FormatId.Parse("audio.wav"), FileFamilyId.Parse("audio"), "WAV", ".wav", [".wav"]);
        var capability = new CapabilityDescriptor(ProviderId.Parse("fake.audio"), FormatId.Parse("audio.wav"), FormatId.Parse("audio.mp3"), ConversionMode.Transcode, 100);
        Assert.Equal(SchemaVersions.Current, format.SchemaVersion);
        Assert.Equal(SchemaVersions.Current, capability.SchemaVersion);
    }

    [Fact]
    public void RequestAndProbePathsRejectEmbeddedNul()
    {
        var badPath = "chapter\0.wav";
        Assert.Throws<ArgumentException>(() => new ConversionRequest(SchemaVersions.Current, Guid.NewGuid(), ConversionAction.ConvertUsingDefault, [badPath], null, null));
        Assert.Throws<ArgumentException>(() => new ProbedFileDescriptor(badPath, FileFamilyId.Parse("audio"), FormatId.Parse("audio.wav"), 1));
    }
}
