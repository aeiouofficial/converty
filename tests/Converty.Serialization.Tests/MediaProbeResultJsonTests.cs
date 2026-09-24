using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using Converty.Contracts.Conversion;
using Converty.Serialization;

namespace Converty.Serialization.Tests;

public sealed class MediaProbeResultJsonTests
{
    [Fact]
    public void SuccessResultRoundTripsWithoutRawBackendText()
    {
        MediaProbeResultV1 result = MediaProbeResultV1.Success(CreateFacts());

        string json = Serialize(result);
        Assert.Contains("\"schemaVersion\":1", json, StringComparison.Ordinal);
        Assert.Contains("\"status\":\"success\"", json, StringComparison.Ordinal);
        Assert.Contains("\"container\":\"mp4\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("tags", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ffprobe", json, StringComparison.OrdinalIgnoreCase);

        MediaProbeResultV1 roundTrip = Deserialize(json);
        Assert.Equal(MediaProbeStatus.Success, roundTrip.Status);
        Assert.Equal(MediaProbeFailureReason.None, roundTrip.FailureReason);
        Assert.NotNull(roundTrip.Facts);
        Assert.Equal(MediaContainerId.Mp4, roundTrip.Facts!.Container);
        Assert.Single(roundTrip.Facts.Streams);
        Assert.Equal(MediaCodecId.H264, roundTrip.Facts.Streams[0].Codec);
    }

    [Fact]
    public void FailureResultRoundTripsWithoutFacts()
    {
        MediaProbeResultV1 result = MediaProbeResultV1.Failure(MediaProbeFailureReason.Timeout);

        string json = Serialize(result);
        Assert.Contains("\"status\":\"failure\"", json, StringComparison.Ordinal);
        Assert.Contains("\"failureReason\":\"timeout\"", json, StringComparison.Ordinal);

        MediaProbeResultV1 roundTrip = Deserialize(json);
        Assert.Equal(MediaProbeStatus.Failure, roundTrip.Status);
        Assert.Null(roundTrip.Facts);
        Assert.Equal(MediaProbeFailureReason.Timeout, roundTrip.FailureReason);
    }

    [Theory]
    [InlineData("{\"schemaVersion\":2,\"status\":\"failure\",\"failureReason\":\"timeout\"}")]
    [InlineData("{\"schemaVersion\":1,\"status\":\"failure\",\"status\":\"success\",\"failureReason\":\"timeout\"}")]
    [InlineData("{\"schemaVersion\":1,\"status\":\"failure\",\"failureReason\":\"timeout\",\"command\":\"calc.exe\"}")]
    [InlineData("{\"schemaVersion\":1,\"status\":\"failure\",\"failureReason\":\"timeout\"} trailing")]
    [InlineData("{\"schemaVersion\":1,\"status\":\"failure\",\"failureReason\":\"timeout\"")]
    public void MalformedFutureDuplicateExtraAndTrailingJsonReject(string json)
    {
        Assert.ThrowsAny<JsonException>(() => Deserialize(json));
    }

    [Fact]
    public void MissingRequiredStreamMemberRejectsButExplicitUnknownIsPreserved()
    {
        string missingCodec = SuccessJson(StreamJson().Replace(",\"codec\":\"h264\"", string.Empty, StringComparison.Ordinal));
        Assert.Throws<JsonException>(() => Deserialize(missingCodec));

        string explicitUnknown = SuccessJson(StreamJson().Replace("\"codec\":\"h264\"", "\"codec\":\"unknown\"", StringComparison.Ordinal));
        MediaProbeResultV1 result = Deserialize(explicitUnknown);
        Assert.Equal(MediaCodecId.Unknown, result.Facts!.Streams[0].Codec);
    }

    [Fact]
    public void StreamCountMaxSucceedsAndMaxPlusOneRejects()
    {
        string[] streams = Enumerable.Range(0, MediaProbeFactsV1.MaximumStreams)
            .Select(index => StreamJson(index))
            .ToArray();
        _ = Deserialize(SuccessJson(string.Join(',', streams)));

        string tooMany = string.Join(',', streams.Append(StreamJson(MediaProbeFactsV1.MaximumStreams)));
        Assert.Throws<JsonException>(() => Deserialize(SuccessJson(tooMany)));
    }

    [Theory]
    [InlineData("\"width\":32769")]
    [InlineData("\"bitDepth\":17")]
    public void ExtremeNumericFactsReject(string replacement)
    {
        string stream = StreamJson()
            .Replace("\"width\":1920", replacement, StringComparison.Ordinal);
        Assert.Throws<JsonException>(() => Deserialize(SuccessJson(stream)));
    }

    [Fact]
    public void ContradictoryHdrFactsReject()
    {
        string stream = StreamJson()
            .Replace("\"colorTransfer\":\"bt709\"", "\"colorTransfer\":\"smpte2084\"", StringComparison.Ordinal);
        Assert.Throws<JsonException>(() => Deserialize(SuccessJson(stream)));
    }

    private static MediaProbeFactsV1 CreateFacts() => new(
        MediaContainerId.Mp4,
        new[]
        {
            new MediaStreamFactsV1(
                0,
                MediaStreamKind.Video,
                MediaCodecId.H264,
                MediaProfileId.H264High,
                true,
                false,
                MediaPixelFormatId.Yuv420p,
                8,
                1920,
                1080,
                MediaColorTransferId.Bt709,
                MediaHdrState.Sdr,
                null,
                null,
                MediaAudioChannelLayoutId.Unknown,
                false),
        },
        MediaProbeCompleteness.Complete,
        false,
        false,
        false);

    private static string SuccessJson(string? streams = null) =>
        "{\"schemaVersion\":1,\"status\":\"success\",\"facts\":{\"container\":\"mp4\",\"streams\":[" +
        (streams ?? StreamJson()) +
        "],\"completeness\":\"complete\",\"hasChapters\":false,\"hasGlobalMetadata\":false,\"hasPolicyRelevantStreamMetadata\":false},\"failureReason\":\"none\"}";

    private static string StreamJson(int index = 0) =>
        "{\"index\":" + index +
        ",\"kind\":\"video\",\"codec\":\"h264\",\"profile\":\"h264High\",\"isDefault\":true,\"isAttachedPicture\":false,\"pixelFormat\":\"yuv420p\",\"bitDepth\":8,\"width\":1920,\"height\":1080,\"colorTransfer\":\"bt709\",\"hdrState\":\"sdr\",\"channelLayout\":\"unknown\",\"hasPolicyRelevantMetadata\":false}";

    private static string Serialize(MediaProbeResultV1 result)
    {
        MethodInfo method = Assert.Single(
            typeof(ContractJson).GetMethods(BindingFlags.Public | BindingFlags.Static),
            candidate => candidate.Name == "Serialize"
                && candidate.GetParameters().Length == 1
                && candidate.GetParameters()[0].ParameterType == typeof(MediaProbeResultV1));
        return (string)Invoke(method, null, result)!;
    }

    private static MediaProbeResultV1 Deserialize(string json)
    {
        MethodInfo method = Assert.Single(
            typeof(ContractJson).GetMethods(BindingFlags.Public | BindingFlags.Static),
            candidate => candidate.Name == "DeserializeMediaProbeResult"
                && candidate.GetParameters().Length == 1
                && candidate.GetParameters()[0].ParameterType == typeof(string));
        return (MediaProbeResultV1)Invoke(method, null, json)!;
    }

    private static object? Invoke(MethodInfo method, object? target, params object?[] arguments)
    {
        try
        {
            return method.Invoke(target, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
