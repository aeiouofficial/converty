using System.Text.Json;
using FileConvert.Serialization;

namespace FileConvert.Serialization.Tests;

public sealed class AdversarialJsonTests
{
    [Fact]
    public void UnknownSchemaVersionIsRejectedBeforeContractMapping()
    {
        const string json = """
        {"schemaVersion":2,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"]}
        """;
        var exception = Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
        Assert.Contains("Unsupported schema version 2", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingSchemaVersionIsRejected()
    {
        const string json = """
        {"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void UnknownMemberIsRejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"],"command":"calc.exe"}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void PropertyNamesAreCaseSensitive()
    {
        const string json = """
        {"SchemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void UnknownEnumTextIsRejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"runCommand","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void ConditionalRequestFieldsAreRevalidatedByDomainContract()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"],"targetFormat":"audio.mp3"}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void TrailingCommaIsRejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"],}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void CommentsAreRejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111",/*x*/"action":"convertUsingDefault","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void OverlongPresetDisplayNameIsRejectedAfterWireParse()
    {
        var displayName = new string('x', 129);
        var json = $$"""
        {"schemaVersion":1,"id":"audio.mp3.high","displayName":"{{displayName}}","familyId":"audio","outputFormat":"audio.mp3","options":{}}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionPreset(json));
    }

    [Fact]
    public void SeededUnknownMemberMutationsAreRejected()
    {
        var random = new Random(0x51A1E);
        const string prefix = "{\"schemaVersion\":1,\"requestId\":\"11111111-1111-1111-1111-111111111111\",\"action\":\"convertUsingDefault\",\"files\":[\"a.wav\"]";
        for (var iteration = 0; iteration < 500; iteration++)
        {
            var member = $"unknown{random.Next(0, int.MaxValue):x8}";
            var json = $"{prefix},\"{member}\":true}}";
            Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
        }
    }

    [Fact]
    public void DuplicateMembersAreRejectedInsteadOfUsingLastValue()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","action":"convertToFormat","files":["a.wav"],"targetFormat":"audio.mp3"}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void DuplicateNestedOptionMembersAreRejected()
    {
        const string json = """
        {"schemaVersion":1,"id":"audio.mp3.high","displayName":"High","familyId":"audio","outputFormat":"audio.mp3","options":{"quality":"high","quality":"low"}}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionPreset(json));
    }
}
