using System.Text.Json;
using FileConvert.Serialization;

namespace FileConvert.Serialization.Tests;

public sealed class AdversarialJsonTests
{
    [Fact]
    public void Unknown_schema_version_is_rejected_before_contract_mapping()
    {
        const string json = """
        {"schemaVersion":2,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"]}
        """;
        var exception = Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
        Assert.Contains("Unsupported schema version 2", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_schema_version_is_rejected()
    {
        const string json = """
        {"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Unknown_member_is_rejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"],"command":"calc.exe"}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Property_names_are_case_sensitive()
    {
        const string json = """
        {"SchemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Unknown_enum_text_is_rejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"runCommand","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Conditional_request_fields_are_revalidated_by_domain_contract()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"],"targetFormat":"audio.mp3"}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Trailing_comma_is_rejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","files":["a.wav"],}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Comments_are_rejected()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111",/*x*/"action":"convertUsingDefault","files":["a.wav"]}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Overlong_preset_display_name_is_rejected_after_wire_parse()
    {
        var displayName = new string('x', 129);
        var json = $$"""
        {"schemaVersion":1,"id":"audio.mp3.high","displayName":"{{displayName}}","familyId":"audio","outputFormat":"audio.mp3","options":{}}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionPreset(json));
    }

    [Fact]
    public void Seeded_unknown_member_mutations_are_rejected()
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
    public void Duplicate_members_are_rejected_instead_of_using_last_value()
    {
        const string json = """
        {"schemaVersion":1,"requestId":"11111111-1111-1111-1111-111111111111","action":"convertUsingDefault","action":"convertToFormat","files":["a.wav"],"targetFormat":"audio.mp3"}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionRequest(json));
    }

    [Fact]
    public void Duplicate_nested_option_members_are_rejected()
    {
        const string json = """
        {"schemaVersion":1,"id":"audio.mp3.high","displayName":"High","familyId":"audio","outputFormat":"audio.mp3","options":{"quality":"high","quality":"low"}}
        """;
        Assert.Throws<JsonException>(() => ContractJson.DeserializeConversionPreset(json));
    }
}
