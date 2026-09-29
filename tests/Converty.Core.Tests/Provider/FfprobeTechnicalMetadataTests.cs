using Converty.Contracts.Conversion;
using Converty.Provider.FFmpeg;

namespace Converty.Core.Tests.Provider;

public sealed class FfprobeTechnicalMetadataTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "converty-ffprobe-metadata-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void KnownMuxerGeneratedTechnicalTagsAreNotPolicyRelevant()
    {
        Directory.CreateDirectory(_root);

        MediaProbeFactsV1 webm = Parse(
            "output.webm",
            """
            {
              "streams": [
                {
                  "index": 0,
                  "codec_name": "vp9",
                  "profile": "Profile 0",
                  "codec_type": "video",
                  "width": 64,
                  "height": 48,
                  "pix_fmt": "yuv420p",
                  "color_transfer": "bt709",
                  "disposition": { "default": 1, "attached_pic": 0 },
                  "tags": {
                    "ENCODER": "Lavc63.7.100 libvpx-vp9",
                    "DURATION": "00:00:00.500000000"
                  }
                },
                {
                  "index": 1,
                  "codec_name": "opus",
                  "codec_type": "audio",
                  "sample_rate": "48000",
                  "channels": 2,
                  "channel_layout": "stereo",
                  "disposition": { "default": 1, "attached_pic": 0 },
                  "tags": {
                    "ENCODER": "Lavc63.7.100 libopus",
                    "DURATION": "00:00:00.518000000"
                  }
                }
              ],
              "chapters": [],
              "format": {
                "format_name": "matroska,webm",
                "tags": { "ENCODER": "Lavf63.2.100" }
              }
            }
            """);

        Assert.False(webm.HasGlobalMetadata);
        Assert.False(webm.HasPolicyRelevantStreamMetadata);
        Assert.All(webm.Streams, stream => Assert.False(stream.HasPolicyRelevantMetadata));

        MediaProbeFactsV1 mp4 = Parse(
            "output.mp4",
            """
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
                  "color_transfer": "bt709",
                  "disposition": { "default": 1, "attached_pic": 0 },
                  "tags": {
                    "language": "und",
                    "handler_name": "VideoHandler",
                    "vendor_id": "[0][0][0][0]",
                    "encoder": "Lavc63.7.100 libx264"
                  }
                },
                {
                  "index": 1,
                  "codec_name": "aac",
                  "codec_type": "audio",
                  "sample_rate": "48000",
                  "channels": 2,
                  "channel_layout": "stereo",
                  "disposition": { "default": 1, "attached_pic": 0 },
                  "tags": {
                    "language": "und",
                    "handler_name": "SoundHandler",
                    "vendor_id": "[0][0][0][0]"
                  }
                }
              ],
              "chapters": [],
              "format": {
                "format_name": "mov,mp4,m4a,3gp,3g2,mj2",
                "tags": {
                  "major_brand": "isom",
                  "minor_version": "512",
                  "compatible_brands": "isomiso2avc1mp41",
                  "encoder": "Lavf63.2.100"
                }
              }
            }
            """);

        Assert.False(mp4.HasGlobalMetadata);
        Assert.False(mp4.HasPolicyRelevantStreamMetadata);
        Assert.All(mp4.Streams, stream => Assert.False(stream.HasPolicyRelevantMetadata));
    }

    [Fact]
    public void TechnicalTagNamesWithUntrustedValuesRemainPolicyRelevant()
    {
        Directory.CreateDirectory(_root);

        MediaProbeFactsV1 facts = Parse(
            "input.mp4",
            """
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
                  "color_transfer": "bt709",
                  "disposition": { "default": 1, "attached_pic": 0 },
                  "tags": { "language": "user-selected-language" }
                }
              ],
              "chapters": [],
              "format": {
                "format_name": "mov,mp4,m4a,3gp,3g2,mj2",
                "tags": { "encoder": "user supplied value" }
              }
            }
            """);

        Assert.True(facts.HasGlobalMetadata);
        Assert.True(facts.HasPolicyRelevantStreamMetadata);
        Assert.True(facts.Streams[0].HasPolicyRelevantMetadata);
    }

    private MediaProbeFactsV1 Parse(string fileName, string rawJson)
    {
        string input = Path.Combine(_root, fileName);
        File.WriteAllBytes(input, [0x00]);
        return FfprobeJsonAdapter.Parse(input, rawJson);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
