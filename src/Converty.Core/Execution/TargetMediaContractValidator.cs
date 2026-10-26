using Converty.Contracts.Conversion;

namespace Converty.Core.Execution;

public static class TargetMediaContractValidator
{
    public static void Validate(TargetMediaContract contract, MediaProbeFactsV1 facts)
    {
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(facts);

        if (facts.Completeness != MediaProbeCompleteness.Complete)
        {
            throw new InvalidOperationException("Target probe facts are incomplete.");
        }

        if (facts.Container != contract.Container)
        {
            throw new InvalidOperationException("Target container does not match the selected contract.");
        }

        if (facts.HasChapters != contract.HasChapters
            || facts.HasGlobalMetadata != contract.HasGlobalMetadata
            || facts.HasPolicyRelevantStreamMetadata != contract.HasPolicyRelevantStreamMetadata)
        {
            throw new InvalidOperationException("Target metadata or chapter state does not match the selected contract.");
        }

        if (facts.Streams.Any(stream => stream.Kind is MediaStreamKind.Unknown
                or MediaStreamKind.Subtitle
                or MediaStreamKind.Data
                or MediaStreamKind.Attachment))
        {
            throw new InvalidOperationException("Target contains a stream type excluded by the selected contract.");
        }

        if (!contract.HasPolicyRelevantStreamMetadata
            && facts.Streams.Any(stream => stream.HasPolicyRelevantMetadata))
        {
            throw new InvalidOperationException("Target contains policy-relevant stream metadata.");
        }

        MediaStreamFactsV1[] videoStreams = facts.Streams
            .Where(stream => stream.Kind == MediaStreamKind.Video)
            .ToArray();
        MediaStreamFactsV1[] audioStreams = facts.Streams
            .Where(stream => stream.Kind == MediaStreamKind.Audio)
            .ToArray();

        if (contract.ExpectsVideo)
        {
            if (videoStreams.Length != 1 || videoStreams[0].IsAttachedPicture)
            {
                throw new InvalidOperationException("Target Video topology does not match the selected contract.");
            }

            ValidateVideo(contract, videoStreams[0]);
        }
        else if (videoStreams.Length != 0)
        {
            throw new InvalidOperationException("Target unexpectedly contains Video content.");
        }

        if (contract.ExpectsAudio)
        {
            if (audioStreams.Length != 1)
            {
                throw new InvalidOperationException("Target Audio topology does not match the selected contract.");
            }

            ValidateAudio(contract, audioStreams[0]);
        }
        else if (audioStreams.Length != 0)
        {
            throw new InvalidOperationException("Target unexpectedly contains Audio content.");
        }
    }

    private static void ValidateVideo(TargetMediaContract contract, MediaStreamFactsV1 stream)
    {
        if (stream.Codec != contract.VideoCodec
            || stream.PixelFormat != contract.VideoPixelFormat
            || stream.BitDepth != contract.VideoBitDepth
            || stream.ColorTransfer != contract.VideoColorTransfer
            || stream.HdrState != contract.VideoHdrState)
        {
            throw new InvalidOperationException("Target Video stream does not match the selected codec and compatibility contract.");
        }

        if (stream.Width is null || stream.Height is null)
        {
            throw new InvalidOperationException("Target Video dimensions are missing.");
        }
    }

    private static void ValidateAudio(TargetMediaContract contract, MediaStreamFactsV1 stream)
    {
        if (stream.Codec != contract.AudioCodec
            || stream.SampleRate != contract.AudioSampleRate
            || stream.ChannelCount != contract.AudioChannelCount
            || stream.ChannelLayout != contract.AudioChannelLayout)
        {
            throw new InvalidOperationException("Target Audio stream does not match the selected compatibility contract.");
        }
    }
}
