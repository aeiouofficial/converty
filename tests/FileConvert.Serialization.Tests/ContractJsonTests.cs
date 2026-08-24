using FileConvert.Contracts;
using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;
using FileConvert.Contracts.Jobs;
using FileConvert.Serialization;

namespace FileConvert.Serialization.Tests;

public sealed class ContractJsonTests
{
    [Fact]
    public void ConversionRequest_round_trips_with_explicit_wire_action()
    {
        var source = new ConversionRequest(SchemaVersions.Current, Guid.Parse("11111111-1111-1111-1111-111111111111"), ConversionAction.ConvertToFormat, [@"C:\Media\chapter.wav"], FormatId.Parse("audio.mp3"), null);
        var json = ContractJson.Serialize(source);
        var result = ContractJson.DeserializeConversionRequest(json);
        Assert.Contains("\"action\":\"convertToFormat\"", json, StringComparison.Ordinal);
        Assert.Equal(source.SchemaVersion, result.SchemaVersion);
        Assert.Equal(source.RequestId, result.RequestId);
        Assert.Equal(source.Action, result.Action);
        Assert.Equal(source.Files, result.Files);
        Assert.Equal(source.TargetFormat, result.TargetFormat);
        Assert.Null(result.PresetId);
    }

    [Fact]
    public void ConversionPreset_round_trips_and_serializes_options_in_ordinal_key_order()
    {
        var source = new ConversionPreset(SchemaVersions.Current, PresetId.Parse("audio.mp3.high"), "MP3 — High Quality", FileFamilyId.Parse("audio"), FormatId.Parse("audio.mp3"), ProviderId.Parse("ffmpeg.audio"), new Dictionary<string, string>(StringComparer.Ordinal) { ["zeta"] = "last", ["alpha"] = "first" });
        var json = ContractJson.Serialize(source);
        var result = ContractJson.DeserializeConversionPreset(json);
        Assert.True(json.IndexOf("alpha", StringComparison.Ordinal) < json.IndexOf("zeta", StringComparison.Ordinal));
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.DisplayName, result.DisplayName);
        Assert.Equal(source.FamilyId, result.FamilyId);
        Assert.Equal(source.OutputFormat, result.OutputFormat);
        Assert.Equal(source.PreferredProvider, result.PreferredProvider);
        Assert.Equal(source.Options.Count, result.Options.Count);
        foreach (var pair in source.Options) { Assert.True(result.Options.TryGetValue(pair.Key, out var actual)); Assert.Equal(pair.Value, actual); }
    }

    [Fact]
    public void CapabilityDescriptor_round_trips_with_schema_version()
    {
        var source = new CapabilityDescriptor(ProviderId.Parse("fake.audio"), FormatId.Parse("audio.wav"), FormatId.Parse("audio.mp3"), ConversionMode.Transcode, 100);
        var result = ContractJson.DeserializeCapabilityDescriptor(ContractJson.Serialize(source));
        Assert.Equal(source, result);
        Assert.Equal(SchemaVersions.Current, result.SchemaVersion);
    }

    [Fact]
    public void FormatDescriptor_round_trips_with_normalized_extensions()
    {
        var source = new FormatDescriptor(FormatId.Parse("audio.wav"), FileFamilyId.Parse("audio"), "WAV", ".wav", [".WAV", ".wave"]);
        var result = ContractJson.DeserializeFormatDescriptor(ContractJson.Serialize(source));
        Assert.Equal(source.SchemaVersion, result.SchemaVersion);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.FamilyId, result.FamilyId);
        Assert.Equal(source.DisplayName, result.DisplayName);
        Assert.Equal(source.CanonicalExtension, result.CanonicalExtension);
        Assert.Equal(source.Extensions, result.Extensions);
    }

    [Fact]
    public void ConversionPlan_round_trips_nested_probe_without_executable_configuration()
    {
        var source = new ConversionPlan(SchemaVersions.Current, Guid.Parse("22222222-2222-2222-2222-222222222222"), new ProbedFileDescriptor(@"C:\Media\chapter.wav", FileFamilyId.Parse("audio"), FormatId.Parse("audio.wav"), 123456), FormatId.Parse("audio.mp3"), ProviderId.Parse("fake.audio"), ConversionMode.Transcode, PresetId.Parse("audio.mp3.high"));
        var result = ContractJson.DeserializeConversionPlan(ContractJson.Serialize(source));
        Assert.Equal(source.RequestId, result.RequestId);
        Assert.Equal(source.Source.Path, result.Source.Path);
        Assert.Equal(source.Source.FamilyId, result.Source.FamilyId);
        Assert.Equal(source.Source.FormatId, result.Source.FormatId);
        Assert.Equal(source.Source.Length, result.Source.Length);
        Assert.Equal(source.TargetFormat, result.TargetFormat);
        Assert.Equal(source.ProviderId, result.ProviderId);
        Assert.Equal(source.Mode, result.Mode);
        Assert.Equal(source.PresetId, result.PresetId);
    }

    [Fact]
    public void JobStatusSnapshot_round_trips_state_progress_and_message()
    {
        var source = new JobStatusSnapshot(SchemaVersions.Current, Guid.Parse("33333333-3333-3333-3333-333333333333"), Guid.Parse("44444444-4444-4444-4444-444444444444"), ConversionJobState.Converting, 0.5, "Worker is running.");
        var json = ContractJson.Serialize(source);
        var result = ContractJson.DeserializeJobStatusSnapshot(json);
        Assert.Contains("\"state\":\"converting\"", json, StringComparison.Ordinal);
        Assert.Equal(source.JobId, result.JobId);
        Assert.Equal(source.RequestId, result.RequestId);
        Assert.Equal(source.State, result.State);
        Assert.Equal(source.Progress, result.Progress);
        Assert.Equal(source.Message, result.Message);
    }
}
