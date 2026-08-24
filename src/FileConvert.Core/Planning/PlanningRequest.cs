using FileConvert.Contracts.Conversion;
using FileConvert.Contracts.Identifiers;

namespace FileConvert.Core.Planning;

public sealed class PlanningRequest
{
    public PlanningRequest(
        Guid requestId,
        ProbedFileDescriptor source,
        FormatId targetFormat,
        ProviderId? preferredProvider = null,
        PresetId? presetId = null,
        bool allowIdentity = false)
    {
        if (requestId == Guid.Empty)
        {
            throw new ArgumentException("Request ID must not be empty.", nameof(requestId));
        }

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(targetFormat);

        RequestId = requestId;
        Source = source;
        TargetFormat = targetFormat;
        PreferredProvider = preferredProvider;
        PresetId = presetId;
        AllowIdentity = allowIdentity;
    }

    public Guid RequestId { get; }
    public ProbedFileDescriptor Source { get; }
    public FormatId TargetFormat { get; }
    public ProviderId? PreferredProvider { get; }
    public PresetId? PresetId { get; }
    public bool AllowIdentity { get; }
}
