using System.Collections.ObjectModel;

namespace Converty.Contracts.Conversion;

public sealed class MediaProbeFactsV1
{
    public const int MaximumStreams = 32;

    public MediaProbeFactsV1(
        MediaContainerId container,
        IEnumerable<MediaStreamFactsV1> streams,
        MediaProbeCompleteness completeness,
        bool hasChapters,
        bool hasGlobalMetadata,
        bool hasPolicyRelevantStreamMetadata)
    {
        if (!Enum.IsDefined(container))
        {
            throw new ArgumentOutOfRangeException(nameof(container));
        }

        if (!Enum.IsDefined(completeness))
        {
            throw new ArgumentOutOfRangeException(nameof(completeness));
        }

        ArgumentNullException.ThrowIfNull(streams);
        MediaStreamFactsV1[] snapshot = streams.ToArray();
        if (snapshot.Length > MaximumStreams)
        {
            throw new ArgumentException($"A probe result may contain at most {MaximumStreams} streams.", nameof(streams));
        }

        if (snapshot.Any(stream => stream is null))
        {
            throw new ArgumentException("Probe streams cannot contain null entries.", nameof(streams));
        }

        HashSet<int> indexes = new();
        foreach (MediaStreamFactsV1 stream in snapshot)
        {
            if (!indexes.Add(stream.Index))
            {
                throw new ArgumentException("Probe stream indexes must be unique.", nameof(streams));
            }
        }

        Container = container;
        Streams = Array.AsReadOnly(snapshot);
        Completeness = completeness;
        HasChapters = hasChapters;
        HasGlobalMetadata = hasGlobalMetadata;
        HasPolicyRelevantStreamMetadata = hasPolicyRelevantStreamMetadata;
    }

    public MediaContainerId Container { get; }
    public ReadOnlyCollection<MediaStreamFactsV1> Streams { get; }
    public MediaProbeCompleteness Completeness { get; }
    public bool HasChapters { get; }
    public bool HasGlobalMetadata { get; }
    public bool HasPolicyRelevantStreamMetadata { get; }
}
