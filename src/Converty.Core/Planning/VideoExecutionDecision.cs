using Converty.Contracts.Conversion;
using Converty.Contracts.Identifiers;

namespace Converty.Core.Planning;

public sealed class VideoExecutionDecision
{
    private VideoExecutionDecision(
        bool isAllowed,
        ConversionMode? mode,
        VideoPlanningReasonCode reasonCode,
        PresetId targetContractId)
    {
        ArgumentNullException.ThrowIfNull(targetContractId);
        if (!Enum.IsDefined(reasonCode))
        {
            throw new ArgumentOutOfRangeException(nameof(reasonCode));
        }

        if (isAllowed)
        {
            if (mode is not (ConversionMode.Copy or ConversionMode.Remux or ConversionMode.Transcode))
            {
                throw new ArgumentException("Allowed Video decisions must select Copy, Remux, or Transcode.", nameof(mode));
            }
        }
        else if (mode is not null)
        {
            throw new ArgumentException("Rejected Video decisions must not select an execution mode.", nameof(mode));
        }

        IsAllowed = isAllowed;
        Mode = mode;
        ReasonCode = reasonCode;
        TargetContractId = targetContractId;
    }

    public bool IsAllowed { get; }
    public ConversionMode? Mode { get; }
    public VideoPlanningReasonCode ReasonCode { get; }
    public PresetId TargetContractId { get; }

    internal static VideoExecutionDecision Allow(
        ConversionMode mode,
        VideoPlanningReasonCode reasonCode,
        PresetId targetContractId)
        => new(true, mode, reasonCode, targetContractId);

    internal static VideoExecutionDecision Reject(
        VideoPlanningReasonCode reasonCode,
        PresetId targetContractId)
        => new(false, null, reasonCode, targetContractId);
}
