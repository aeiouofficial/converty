namespace Converty.Contracts.Conversion;

public enum MediaContainerId
{
    Unknown,
    Mp4,
    Mov,
    Matroska,
    Avi,
    WebM,
    Mpeg,
    Wmv,
    Mp3,
}

public enum MediaStreamKind
{
    Unknown,
    Video,
    Audio,
    Subtitle,
    Data,
    Attachment,
}

public enum MediaCodecId
{
    Unknown,
    H264,
    Vp9,
    Mpeg4,
    Mpeg2Video,
    Wmv2,
    Aac,
    Opus,
    Mp3,
    Mp2,
    Wmav2,
    OtherKnown,
}

public enum MediaProfileId
{
    Unknown,
    H264Baseline,
    H264Main,
    H264High,
    Vp9Profile0,
    OtherKnown,
}

public enum MediaPixelFormatId
{
    Unknown,
    Yuv420p,
    OtherKnown,
}

public enum MediaColorTransferId
{
    Unknown,
    Bt709,
    Smpte2084,
    AribStdB67,
    OtherKnown,
}

public enum MediaHdrState
{
    Unknown,
    Sdr,
    Hdr,
}

public enum MediaAudioChannelLayoutId
{
    Unknown,
    Mono,
    Stereo,
    Multichannel,
    OtherKnown,
}

public enum MediaProbeCompleteness
{
    Incomplete,
    Complete,
}

public enum MediaProbeStatus
{
    Unknown,
    Success,
    Failure,
}

public enum MediaProbeFailureReason
{
    None,
    ProbeFailed,
    Timeout,
    OutputLimitExceeded,
    MalformedOutput,
    UnsupportedInput,
}
