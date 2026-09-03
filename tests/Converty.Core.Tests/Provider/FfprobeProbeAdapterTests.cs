using System.Diagnostics;
using System.Text.Json;
using Converty.Contracts.Conversion;
using Converty.Provider.FFmpeg;

namespace Converty.Core.Tests.Provider;

public sealed class FfprobeProbeAdapterTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "converty-ffprobe-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void TrustedFfprobePathResolvesOnlyFixedBundledToolsLocation()
    {
        string engineDirectory = Path.Combine(_root, "tools", "ffmpeg");
        Directory.CreateDirectory(engineDirectory);
        string expected = Path.Combine(engineDirectory, "ffprobe.exe");
        File.WriteAllBytes(expected, [0x4d, 0x5a]);
        File.WriteAllBytes(Path.Combine(_root, "ffprobe.exe"), [0x4d, 0x5a]);

        Assert.Equal(expected, TrustedFfprobePath.Resolve(_root));
    }

    [Fact]
    public void LauncherUsesDirectFixedFileOnlyArgumentVectorAndKeepsInputAsOneToken()
    {
        string engineDirectory = Path.Combine(_root, "tools", "ffmpeg");
        Directory.CreateDirectory(engineDirectory);
        string ffprobe = Path.Combine(engineDirectory, "ffprobe.exe");
        string input = Path.Combine(_root, "video ü ; $ (probe).mp4");
        File.WriteAllBytes(ffprobe, [0x4d, 0x5a]);
        File.WriteAllBytes(input, [0x00]);

        ProcessStartInfo startInfo = FfprobeProcessLauncher.CreateStartInfo(ffprobe, input);

        Assert.Equal(ffprobe, startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.True(startInfo.CreateNoWindow);
        Assert.True(startInfo.RedirectStandardOutput);
        Assert.True(startInfo.RedirectStandardError);
        Assert.Equal(engineDirectory, startInfo.WorkingDirectory);
        Assert.Equal(
            ["-v", "error", "-show_format", "-show_streams", "-show_chapters", "-of", "json", "-protocol_whitelist", "file", input],
            startInfo.ArgumentList);
    }

    [Fact]
    public void AdapterMapsOnlyClosedSemanticFactsAndDoesNotPreserveBackendMetadataText()
    {
        Directory.CreateDirectory(_root);
        string input = Path.Combine(_root, "input.mp4");
        File.WriteAllBytes(input, [0x00]);
        string raw = """
        {
          "streams": [
            {
              "index": 0,
              "codec_name": "h264",
              "profile": "High",
              "codec_type": "video",
              "width": 64,
              "height": 48,
              "pix_fmt": "yuv420p",
              "bits_per_raw_sample": "8",
              "color_transfer": "bt709",
              "disposition": { "default": 1, "attached_pic": 0 },
              "tags": { "language": "do-not-propagate" }
            },
            {
              "index": 1,
              "codec_name": "aac",
              "codec_type": "audio",
              "sample_rate": "48000",
              "channels": 2,
              "channel_layout": "stereo",
              "disposition": { "default": 1, "attached_pic": 0 }
            }
          ],
          "chapters": [],
          "format": {
            "format_name": "mov,mp4,m4a,3gp,3g2,mj2",
            "tags": { "encoder": "untrusted-backend-text" }
          }
        }
        """;

        MediaProbeFactsV1 facts = FfprobeJsonAdapter.Parse(input, raw);

        Assert.Equal(MediaContainerId.Mp4, facts.Container);
        Assert.Equal(MediaProbeCompleteness.Complete, facts.Completeness);
        Assert.False(facts.HasChapters);
        Assert.True(facts.HasGlobalMetadata);
        Assert.True(facts.HasPolicyRelevantStreamMetadata);
        Assert.Equal(2, facts.Streams.Count);

        MediaStreamFactsV1 video = facts.Streams[0];
        Assert.Equal(MediaStreamKind.Video, video.Kind);
        Assert.Equal(MediaCodecId.H264, video.Codec);
        Assert.Equal(MediaProfileId.H264High, video.Profile);
        Assert.Equal(MediaPixelFormatId.Yuv420p, video.PixelFormat);
        Assert.Equal(8, video.BitDepth);
        Assert.Equal(MediaColorTransferId.Bt709, video.ColorTransfer);
        Assert.Equal(MediaHdrState.Sdr, video.HdrState);
        Assert.True(video.HasPolicyRelevantMetadata);

        MediaStreamFactsV1 audio = facts.Streams[1];
        Assert.Equal(MediaStreamKind.Audio, audio.Kind);
        Assert.Equal(MediaCodecId.Aac, audio.Codec);
        Assert.Equal(48000, audio.SampleRate);
        Assert.Equal(2, audio.ChannelCount);
        Assert.Equal(MediaAudioChannelLayoutId.Stereo, audio.ChannelLayout);
    }

    [Fact]
    public void UnknownBackendValuesCollapseToClosedUnknownAndIncompleteFacts()
    {
        Directory.CreateDirectory(_root);
        string input = Path.Combine(_root, "input.mkv");
        File.WriteAllBytes(input, [0x00]);
        string raw = """
        {
          "streams": [
            {
              "index": 0,
              "codec_name": "future-video-codec-with-attacker-text",
              "codec_type": "video",
              "width": 16,
              "height": 16,
              "pix_fmt": "future-pixfmt",
              "color_transfer": "future-transfer",
              "disposition": { "default": 1, "attached_pic": 0 }
            }
          ],
          "chapters": [],
          "format": { "format_name": "matroska,webm" }
        }
        """;

        MediaProbeFactsV1 facts = FfprobeJsonAdapter.Parse(input, raw);

        Assert.Equal(MediaContainerId.Matroska, facts.Container);
        Assert.Equal(MediaProbeCompleteness.Incomplete, facts.Completeness);
        Assert.Equal(MediaCodecId.Unknown, facts.Streams[0].Codec);
        Assert.Equal(MediaPixelFormatId.Unknown, facts.Streams[0].PixelFormat);
        Assert.Equal(MediaColorTransferId.Unknown, facts.Streams[0].ColorTransfer);
        Assert.Equal(MediaHdrState.Unknown, facts.Streams[0].HdrState);
    }

    [Fact]
    public void MalformedRawProbeJsonFailsClosed()
    {
        Directory.CreateDirectory(_root);
        string input = Path.Combine(_root, "input.webm");
        File.WriteAllBytes(input, [0x00]);

        Assert.ThrowsAny<JsonException>(() => FfprobeJsonAdapter.Parse(input, "{\"streams\":["));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
