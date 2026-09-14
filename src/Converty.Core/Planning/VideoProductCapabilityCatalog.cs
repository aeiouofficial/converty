using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;
using Converty.Core.Capabilities;

namespace Converty.Core.Planning;

public static class VideoProductCapabilityCatalog
{
    public static readonly FileFamilyId VideoFamilyId = FileFamilyId.Parse("video");
    public static readonly ProviderId FfmpegProviderId = ProviderId.Parse("provider.ffmpeg");

    private static readonly FormatId Mp4 = FormatId.Parse("video.mp4");
    private static readonly FormatId Mov = FormatId.Parse("video.mov");
    private static readonly FormatId Matroska = FormatId.Parse("video.mkv");
    private static readonly FormatId Avi = FormatId.Parse("video.avi");
    private static readonly FormatId WebM = FormatId.Parse("video.webm");
    private static readonly FormatId Mpeg = FormatId.Parse("video.mpeg");
    private static readonly FormatId Wmv = FormatId.Parse("video.wmv");
    private static readonly FormatId Mp3 = FormatId.Parse("audio.mp3");

    private static readonly FormatId[] VideoSources = [Mp4, Mov, Matroska, Avi, WebM, Mpeg, Wmv];

    public static ConversionPlanner CreatePlanner() => new(new CapabilityGraph(CreateCapabilities()));

    public static FormatId ResolveSourceFormat(MediaContainerId container) => container switch
    {
        MediaContainerId.Mp4 => Mp4,
        MediaContainerId.Mov => Mov,
        MediaContainerId.Matroska => Matroska,
        MediaContainerId.Avi => Avi,
        MediaContainerId.WebM => WebM,
        MediaContainerId.Mpeg => Mpeg,
        MediaContainerId.Wmv => Wmv,
        _ => throw new InvalidOperationException($"Unsupported Video source container '{container}'."),
    };

    public static FormatId ResolveTargetFormat(PresetId presetId)
    {
        ArgumentNullException.ThrowIfNull(presetId);
        return presetId.Value switch
        {
            "video.mp4.h264" => Mp4,
            "video.webm.vp9" => WebM,
            "extract.audio.mp3" => Mp3,
            _ => throw new InvalidOperationException($"Unsupported Video target preset '{presetId.Value}'."),
        };
    }

    private static IEnumerable<CapabilityDescriptor> CreateCapabilities()
    {
        foreach (FormatId source in VideoSources)
        {
            yield return new CapabilityDescriptor(FfmpegProviderId, source, Mp4, ConversionMode.Remux, 90);
            yield return new CapabilityDescriptor(FfmpegProviderId, source, Mp4, ConversionMode.Transcode, 80);
            yield return new CapabilityDescriptor(FfmpegProviderId, source, WebM, ConversionMode.Remux, 90);
            yield return new CapabilityDescriptor(FfmpegProviderId, source, WebM, ConversionMode.Transcode, 80);
            yield return new CapabilityDescriptor(FfmpegProviderId, source, Mp3, ConversionMode.Remux, 90);
            yield return new CapabilityDescriptor(FfmpegProviderId, source, Mp3, ConversionMode.Transcode, 80);
        }

        yield return new CapabilityDescriptor(FfmpegProviderId, Mp4, Mp4, ConversionMode.Copy, 100);
        yield return new CapabilityDescriptor(FfmpegProviderId, WebM, WebM, ConversionMode.Copy, 100);
    }
}
