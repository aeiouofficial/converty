using System.Collections;
using System.Reflection;
using Converty.Contracts.Conversion;

namespace Converty.Contracts.Tests.Conversion;

public sealed class MediaProbeFactsTests
{
    private const string Namespace = "Converty.Contracts.Conversion.";

    [Fact]
    public void Dev21ProbeContractSurfaceExistsBeforeImplementation()
    {
        Assembly contracts = typeof(ProbedFileDescriptor).Assembly;

        Type factsType = RequireType(contracts, "MediaProbeFactsV1");
        RequireType(contracts, "MediaProbeResultV1");
        RequireType(contracts, "MediaStreamFactsV1");

        ConstructorInfo? additiveConstructor = typeof(ProbedFileDescriptor)
            .GetConstructors()
            .SingleOrDefault(constructor => constructor.GetParameters().Length == 5);
        Assert.NotNull(additiveConstructor);
        Assert.Equal(factsType, additiveConstructor!.GetParameters()[4].ParameterType);

        PropertyInfo? mediaFactsProperty = typeof(ProbedFileDescriptor).GetProperty("MediaFacts");
        Assert.NotNull(mediaFactsProperty);
        Assert.Equal(factsType, mediaFactsProperty!.PropertyType);

        ConstructorInfo? legacyConstructor = typeof(ProbedFileDescriptor)
            .GetConstructors()
            .SingleOrDefault(constructor => constructor.GetParameters().Length == 4);
        Assert.NotNull(legacyConstructor);
    }

    [Fact]
    public void Dev21ProbeEnumsAreClosedCanonicalAndBounded()
    {
        Assembly contracts = typeof(ProbedFileDescriptor).Assembly;

        AssertEnumNames(contracts, "MediaContainerId", "Unknown", "Mp4", "Mov", "Matroska", "Avi", "WebM", "Mpeg", "Wmv", "Mp3");
        AssertEnumNames(contracts, "MediaStreamKind", "Unknown", "Video", "Audio", "Subtitle", "Data", "Attachment");
        AssertEnumNames(contracts, "MediaCodecId", "Unknown", "H264", "Vp9", "Mpeg4", "Mpeg2Video", "Wmv2", "Aac", "Opus", "Mp3", "Mp2", "Wmav2", "OtherKnown");
        AssertEnumNames(contracts, "MediaProfileId", "Unknown", "H264Baseline", "H264Main", "H264High", "Vp9Profile0", "OtherKnown");
        AssertEnumNames(contracts, "MediaPixelFormatId", "Unknown", "Yuv420p", "OtherKnown");
        AssertEnumNames(contracts, "MediaColorTransferId", "Unknown", "Bt709", "Smpte2084", "AribStdB67", "OtherKnown");
        AssertEnumNames(contracts, "MediaHdrState", "Unknown", "Sdr", "Hdr");
        AssertEnumNames(contracts, "MediaAudioChannelLayoutId", "Unknown", "Mono", "Stereo", "Multichannel", "OtherKnown");
        AssertEnumNames(contracts, "MediaProbeCompleteness", "Incomplete", "Complete");
        AssertEnumNames(contracts, "MediaProbeStatus", "Unknown", "Success", "Failure");
        AssertEnumNames(contracts, "MediaProbeFailureReason", "None", "ProbeFailed", "Timeout", "OutputLimitExceeded", "MalformedOutput", "UnsupportedInput");

        Assert.Equal(32, ReadConstant<int>(RequireType(contracts, "MediaProbeFactsV1"), "MaximumStreams"));
        Type streamType = RequireType(contracts, "MediaStreamFactsV1");
        Assert.Equal(1023, ReadConstant<int>(streamType, "MaximumStreamIndex"));
        Assert.Equal(32768, ReadConstant<int>(streamType, "MaximumDimension"));
        Assert.Equal(16, ReadConstant<int>(streamType, "MaximumBitDepth"));
        Assert.Equal(768000, ReadConstant<int>(streamType, "MaximumSampleRate"));
        Assert.Equal(64, ReadConstant<int>(streamType, "MaximumChannels"));
    }

    [Fact]
    public void MediaStreamFactsRejectExtremeAndContradictoryFacts()
    {
        Assembly contracts = typeof(ProbedFileDescriptor).Assembly;

        object video = CreateStream(contracts, index: 0);
        Assert.Equal(0, ReadProperty<int>(video, "Index"));
        Assert.Equal("Video", ReadProperty<object>(video, "Kind").ToString());
        Assert.Equal(1920, ReadNullableIntProperty(video, "Width"));
        Assert.Equal(1080, ReadNullableIntProperty(video, "Height"));

        AssertInvocationThrows<ArgumentOutOfRangeException>(() => CreateStream(contracts, index: 1024));
        AssertInvocationThrows<ArgumentOutOfRangeException>(() => CreateStream(contracts, width: 32769));
        AssertInvocationThrows<ArgumentOutOfRangeException>(() => CreateStream(contracts, bitDepth: 17));
        AssertInvocationThrows<ArgumentException>(() => CreateStream(contracts, colorTransfer: "Smpte2084", hdrState: "Sdr"));
        AssertInvocationThrows<ArgumentException>(() => CreateStream(contracts, kind: "Audio", codec: "Aac", profile: "Unknown", pixelFormat: "Unknown", bitDepth: null, width: 1920, height: 1080, colorTransfer: "Unknown", hdrState: "Unknown", sampleRate: 48000, channelCount: 2, channelLayout: "Stereo"));
        AssertInvocationThrows<ArgumentException>(() => CreateStream(contracts, sampleRate: 48000));
        AssertInvocationThrows<ArgumentException>(() => CreateStream(contracts, kind: "Audio", codec: "Aac", profile: "Unknown", pixelFormat: "Unknown", bitDepth: null, width: null, height: null, colorTransfer: "Unknown", hdrState: "Unknown", sampleRate: 48000, channelCount: 2, channelLayout: "Stereo", isAttachedPicture: true));

        Type kindType = RequireType(contracts, "MediaStreamKind");
        object undefinedKind = Enum.ToObject(kindType, 999);
        AssertInvocationThrows<ArgumentOutOfRangeException>(() => CreateStream(contracts, kindOverride: undefinedKind));
    }

    [Fact]
    public void MediaProbeFactsSnapshotStreamsAndEnforceMaxPlusOneAndUniqueIndexes()
    {
        Assembly contracts = typeof(ProbedFileDescriptor).Assembly;
        Type streamType = RequireType(contracts, "MediaStreamFactsV1");
        Type factsType = RequireType(contracts, "MediaProbeFactsV1");

        object original = CreateStream(contracts, index: 0);
        Array source = Array.CreateInstance(streamType, 1);
        source.SetValue(original, 0);
        object facts = CreateFacts(contracts, source);

        object replacement = CreateStream(contracts, index: 1);
        source.SetValue(replacement, 0);
        object[] captured = ((IEnumerable)ReadProperty<object>(facts, "Streams")).Cast<object>().ToArray();
        Assert.Single(captured);
        Assert.Same(original, captured[0]);
        Assert.Equal(32, ReadConstant<int>(factsType, "MaximumStreams"));

        Array maximum = Array.CreateInstance(streamType, 32);
        for (int i = 0; i < maximum.Length; i++)
        {
            maximum.SetValue(CreateStream(contracts, index: i), i);
        }
        _ = CreateFacts(contracts, maximum);

        Array tooMany = Array.CreateInstance(streamType, 33);
        for (int i = 0; i < tooMany.Length; i++)
        {
            tooMany.SetValue(CreateStream(contracts, index: i), i);
        }
        AssertInvocationThrows<ArgumentException>(() => CreateFacts(contracts, tooMany));

        Array duplicates = Array.CreateInstance(streamType, 2);
        duplicates.SetValue(CreateStream(contracts, index: 7), 0);
        duplicates.SetValue(CreateStream(contracts, index: 7), 1);
        AssertInvocationThrows<ArgumentException>(() => CreateFacts(contracts, duplicates));
    }

    [Fact]
    public void MediaProbeResultUsesVersionedClosedSuccessFailureModel()
    {
        Assembly contracts = typeof(ProbedFileDescriptor).Assembly;
        Type streamType = RequireType(contracts, "MediaStreamFactsV1");
        Array streams = Array.CreateInstance(streamType, 1);
        streams.SetValue(CreateStream(contracts, index: 0), 0);
        object facts = CreateFacts(contracts, streams);

        Type resultType = RequireType(contracts, "MediaProbeResultV1");
        Assert.Equal(1, ReadConstant<int>(resultType, "SchemaVersion"));

        MethodInfo successMethod = Assert.Single(resultType.GetMethods(BindingFlags.Public | BindingFlags.Static), method => method.Name == "Success");
        object success = successMethod.Invoke(null, new[] { facts })!;
        Assert.Equal("Success", ReadProperty<object>(success, "Status").ToString());
        Assert.Same(facts, ReadProperty<object>(success, "Facts"));
        Assert.Equal("None", ReadProperty<object>(success, "FailureReason").ToString());

        MethodInfo failureMethod = Assert.Single(resultType.GetMethods(BindingFlags.Public | BindingFlags.Static), method => method.Name == "Failure");
        Type reasonType = RequireType(contracts, "MediaProbeFailureReason");
        object timeout = EnumValue(reasonType, "Timeout");
        object failure = failureMethod.Invoke(null, new[] { timeout })!;
        Assert.Equal("Failure", ReadProperty<object>(failure, "Status").ToString());
        Assert.Null(resultType.GetProperty("Facts")!.GetValue(failure));
        Assert.Equal("Timeout", ReadProperty<object>(failure, "FailureReason").ToString());

        object none = EnumValue(reasonType, "None");
        AssertInvocationThrows<ArgumentOutOfRangeException>(() => failureMethod.Invoke(null, new[] { none }));
    }

    private static object CreateFacts(Assembly contracts, Array streams)
    {
        Type factsType = RequireType(contracts, "MediaProbeFactsV1");
        ConstructorInfo constructor = Assert.Single(factsType.GetConstructors());
        return constructor.Invoke(new object?[]
        {
            EnumValue(RequireType(contracts, "MediaContainerId"), "Mp4"),
            streams,
            EnumValue(RequireType(contracts, "MediaProbeCompleteness"), "Complete"),
            false,
            false,
            false,
        });
    }

    private static object CreateStream(
        Assembly contracts,
        int index = 0,
        string kind = "Video",
        string codec = "H264",
        string profile = "H264High",
        bool isDefault = true,
        bool isAttachedPicture = false,
        string pixelFormat = "Yuv420p",
        int? bitDepth = 8,
        int? width = 1920,
        int? height = 1080,
        string colorTransfer = "Bt709",
        string hdrState = "Sdr",
        int? sampleRate = null,
        int? channelCount = null,
        string channelLayout = "Unknown",
        bool hasPolicyRelevantMetadata = false,
        object? kindOverride = null)
    {
        Type streamType = RequireType(contracts, "MediaStreamFactsV1");
        ConstructorInfo constructor = Assert.Single(streamType.GetConstructors());
        return constructor.Invoke(new object?[]
        {
            index,
            kindOverride ?? EnumValue(RequireType(contracts, "MediaStreamKind"), kind),
            EnumValue(RequireType(contracts, "MediaCodecId"), codec),
            EnumValue(RequireType(contracts, "MediaProfileId"), profile),
            isDefault,
            isAttachedPicture,
            EnumValue(RequireType(contracts, "MediaPixelFormatId"), pixelFormat),
            bitDepth,
            width,
            height,
            EnumValue(RequireType(contracts, "MediaColorTransferId"), colorTransfer),
            EnumValue(RequireType(contracts, "MediaHdrState"), hdrState),
            sampleRate,
            channelCount,
            EnumValue(RequireType(contracts, "MediaAudioChannelLayoutId"), channelLayout),
            hasPolicyRelevantMetadata,
        });
    }

    private static Type RequireType(Assembly assembly, string shortName)
    {
        Type? type = assembly.GetType(Namespace + shortName);
        Assert.NotNull(type);
        return type!;
    }

    private static object EnumValue(Type enumType, string name) => Enum.Parse(enumType, name, ignoreCase: false);

    private static void AssertEnumNames(Assembly assembly, string shortName, params string[] expected)
    {
        Type enumType = RequireType(assembly, shortName);
        Assert.True(enumType.IsEnum);
        Assert.Equal(expected, Enum.GetNames(enumType));
    }

    private static T ReadConstant<T>(Type declaringType, string name)
    {
        FieldInfo? field = declaringType.GetField(name, BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        return Assert.IsType<T>(field.GetRawConstantValue());
    }

    private static T ReadProperty<T>(object instance, string name)
    {
        PropertyInfo? property = instance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        object? value = property!.GetValue(instance);
        if (value is null)
        {
            return default!;
        }
        return Assert.IsAssignableFrom<T>(value);
    }

    private static int? ReadNullableIntProperty(object instance, string name)
    {
        PropertyInfo? property = instance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(property);
        object? value = property!.GetValue(instance);
        return value is null ? null : Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static TException AssertInvocationThrows<TException>(Action action)
        where TException : Exception
    {
        TargetInvocationException wrapper = Assert.Throws<TargetInvocationException>(action);
        Assert.NotNull(wrapper.InnerException);
        return Assert.IsType<TException>(wrapper.InnerException);
    }
}
