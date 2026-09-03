using System.Text.Json;
using Converty.Contracts.Conversion;

namespace Converty.Serialization.V1;

internal static class ProbeWireEnumText
{
    internal static string ToWire(MediaProbeStatus value) => value switch
    {
        MediaProbeStatus.Success => "success",
        MediaProbeStatus.Failure => "failure",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaProbeStatus ParseStatus(string? value) => value switch
    {
        "success" => MediaProbeStatus.Success,
        "failure" => MediaProbeStatus.Failure,
        _ => throw new JsonException("Invalid media probe status wire value."),
    };

    internal static string ToWire(MediaProbeFailureReason value) => value switch
    {
        MediaProbeFailureReason.None => "none",
        MediaProbeFailureReason.ProbeFailed => "probeFailed",
        MediaProbeFailureReason.Timeout => "timeout",
        MediaProbeFailureReason.OutputLimitExceeded => "outputLimitExceeded",
        MediaProbeFailureReason.MalformedOutput => "malformedOutput",
        MediaProbeFailureReason.UnsupportedInput => "unsupportedInput",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaProbeFailureReason ParseFailureReason(string? value) => value switch
    {
        "none" => MediaProbeFailureReason.None,
        "probeFailed" => MediaProbeFailureReason.ProbeFailed,
        "timeout" => MediaProbeFailureReason.Timeout,
        "outputLimitExceeded" => MediaProbeFailureReason.OutputLimitExceeded,
        "malformedOutput" => MediaProbeFailureReason.MalformedOutput,
        "unsupportedInput" => MediaProbeFailureReason.UnsupportedInput,
        _ => throw new JsonException("Invalid media probe failure reason wire value."),
    };

    internal static string ToWire(MediaContainerId value) => value switch
    {
        MediaContainerId.Unknown => "unknown", MediaContainerId.Mp4 => "mp4", MediaContainerId.Mov => "mov",
        MediaContainerId.Matroska => "matroska", MediaContainerId.Avi => "avi", MediaContainerId.WebM => "webm",
        MediaContainerId.Mpeg => "mpeg", MediaContainerId.Wmv => "wmv", MediaContainerId.Mp3 => "mp3",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaContainerId ParseContainer(string? value) => value switch
    {
        "unknown" => MediaContainerId.Unknown, "mp4" => MediaContainerId.Mp4, "mov" => MediaContainerId.Mov,
        "matroska" => MediaContainerId.Matroska, "avi" => MediaContainerId.Avi, "webm" => MediaContainerId.WebM,
        "mpeg" => MediaContainerId.Mpeg, "wmv" => MediaContainerId.Wmv, "mp3" => MediaContainerId.Mp3,
        _ => throw new JsonException("Invalid media container wire value."),
    };

    internal static string ToWire(MediaStreamKind value) => value switch
    {
        MediaStreamKind.Unknown => "unknown", MediaStreamKind.Video => "video", MediaStreamKind.Audio => "audio",
        MediaStreamKind.Subtitle => "subtitle", MediaStreamKind.Data => "data", MediaStreamKind.Attachment => "attachment",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaStreamKind ParseKind(string? value) => value switch
    {
        "unknown" => MediaStreamKind.Unknown, "video" => MediaStreamKind.Video, "audio" => MediaStreamKind.Audio,
        "subtitle" => MediaStreamKind.Subtitle, "data" => MediaStreamKind.Data, "attachment" => MediaStreamKind.Attachment,
        _ => throw new JsonException("Invalid media stream kind wire value."),
    };

    internal static string ToWire(MediaCodecId value) => value switch
    {
        MediaCodecId.Unknown => "unknown", MediaCodecId.H264 => "h264", MediaCodecId.Vp9 => "vp9",
        MediaCodecId.Mpeg4 => "mpeg4", MediaCodecId.Mpeg2Video => "mpeg2video", MediaCodecId.Wmv2 => "wmv2",
        MediaCodecId.Aac => "aac", MediaCodecId.Opus => "opus", MediaCodecId.Mp3 => "mp3", MediaCodecId.Mp2 => "mp2",
        MediaCodecId.Wmav2 => "wmav2", MediaCodecId.OtherKnown => "otherKnown",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaCodecId ParseCodec(string? value) => value switch
    {
        "unknown" => MediaCodecId.Unknown, "h264" => MediaCodecId.H264, "vp9" => MediaCodecId.Vp9,
        "mpeg4" => MediaCodecId.Mpeg4, "mpeg2video" => MediaCodecId.Mpeg2Video, "wmv2" => MediaCodecId.Wmv2,
        "aac" => MediaCodecId.Aac, "opus" => MediaCodecId.Opus, "mp3" => MediaCodecId.Mp3, "mp2" => MediaCodecId.Mp2,
        "wmav2" => MediaCodecId.Wmav2, "otherKnown" => MediaCodecId.OtherKnown,
        _ => throw new JsonException("Invalid media codec wire value."),
    };

    internal static string ToWire(MediaProfileId value) => value switch
    {
        MediaProfileId.Unknown => "unknown", MediaProfileId.H264Baseline => "h264Baseline", MediaProfileId.H264Main => "h264Main",
        MediaProfileId.H264High => "h264High", MediaProfileId.Vp9Profile0 => "vp9Profile0", MediaProfileId.OtherKnown => "otherKnown",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaProfileId ParseProfile(string? value) => value switch
    {
        "unknown" => MediaProfileId.Unknown, "h264Baseline" => MediaProfileId.H264Baseline, "h264Main" => MediaProfileId.H264Main,
        "h264High" => MediaProfileId.H264High, "vp9Profile0" => MediaProfileId.Vp9Profile0, "otherKnown" => MediaProfileId.OtherKnown,
        _ => throw new JsonException("Invalid media profile wire value."),
    };

    internal static string ToWire(MediaPixelFormatId value) => value switch
    {
        MediaPixelFormatId.Unknown => "unknown", MediaPixelFormatId.Yuv420p => "yuv420p", MediaPixelFormatId.OtherKnown => "otherKnown",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaPixelFormatId ParsePixelFormat(string? value) => value switch
    {
        "unknown" => MediaPixelFormatId.Unknown, "yuv420p" => MediaPixelFormatId.Yuv420p, "otherKnown" => MediaPixelFormatId.OtherKnown,
        _ => throw new JsonException("Invalid media pixel format wire value."),
    };

    internal static string ToWire(MediaColorTransferId value) => value switch
    {
        MediaColorTransferId.Unknown => "unknown", MediaColorTransferId.Bt709 => "bt709", MediaColorTransferId.Smpte2084 => "smpte2084",
        MediaColorTransferId.AribStdB67 => "aribStdB67", MediaColorTransferId.OtherKnown => "otherKnown",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaColorTransferId ParseColorTransfer(string? value) => value switch
    {
        "unknown" => MediaColorTransferId.Unknown, "bt709" => MediaColorTransferId.Bt709, "smpte2084" => MediaColorTransferId.Smpte2084,
        "aribStdB67" => MediaColorTransferId.AribStdB67, "otherKnown" => MediaColorTransferId.OtherKnown,
        _ => throw new JsonException("Invalid media color transfer wire value."),
    };

    internal static string ToWire(MediaHdrState value) => value switch
    {
        MediaHdrState.Unknown => "unknown", MediaHdrState.Sdr => "sdr", MediaHdrState.Hdr => "hdr",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaHdrState ParseHdrState(string? value) => value switch
    {
        "unknown" => MediaHdrState.Unknown, "sdr" => MediaHdrState.Sdr, "hdr" => MediaHdrState.Hdr,
        _ => throw new JsonException("Invalid media HDR state wire value."),
    };

    internal static string ToWire(MediaAudioChannelLayoutId value) => value switch
    {
        MediaAudioChannelLayoutId.Unknown => "unknown", MediaAudioChannelLayoutId.Mono => "mono", MediaAudioChannelLayoutId.Stereo => "stereo",
        MediaAudioChannelLayoutId.Multichannel => "multichannel", MediaAudioChannelLayoutId.OtherKnown => "otherKnown",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaAudioChannelLayoutId ParseChannelLayout(string? value) => value switch
    {
        "unknown" => MediaAudioChannelLayoutId.Unknown, "mono" => MediaAudioChannelLayoutId.Mono, "stereo" => MediaAudioChannelLayoutId.Stereo,
        "multichannel" => MediaAudioChannelLayoutId.Multichannel, "otherKnown" => MediaAudioChannelLayoutId.OtherKnown,
        _ => throw new JsonException("Invalid media channel layout wire value."),
    };

    internal static string ToWire(MediaProbeCompleteness value) => value switch
    {
        MediaProbeCompleteness.Incomplete => "incomplete", MediaProbeCompleteness.Complete => "complete",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static MediaProbeCompleteness ParseCompleteness(string? value) => value switch
    {
        "incomplete" => MediaProbeCompleteness.Incomplete, "complete" => MediaProbeCompleteness.Complete,
        _ => throw new JsonException("Invalid media probe completeness wire value."),
    };
}
