using Converty.Contracts;
using Converty.Contracts.Conversion;
using Converty.Core.Capabilities;

namespace Converty.Core.Planning;

public sealed class ConversionPlanner
{
    private readonly CapabilityGraph _graph;

    public ConversionPlanner(CapabilityGraph graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public ConversionPlan Plan(PlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.AllowIdentity && request.Source.FormatId == request.TargetFormat)
        {
            throw new ConversionPlanningException("Identity conversion is disabled unless explicitly requested.");
        }

        ConversionMode? requiredVideoMode = ResolveRequiredVideoMode(request);

        IEnumerable<CapabilityDescriptor> candidates = _graph.Find(request.Source.FormatId, request.TargetFormat);
        if (request.PreferredProvider is not null)
        {
            candidates = candidates.Where(candidate => candidate.ProviderId == request.PreferredProvider);
        }

        if (requiredVideoMode is not null)
        {
            candidates = candidates.Where(candidate => candidate.Mode == requiredVideoMode.Value);
        }

        var ordered = candidates
            .OrderByDescending(candidate => candidate.Priority)
            .ThenBy(candidate => candidate.ProviderId.Value, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Mode)
            .ToArray();

        if (ordered.Length == 0)
        {
            throw new ConversionPlanningException(
                $"No provider supports {request.Source.FormatId} -> {request.TargetFormat} under the requested policy.");
        }

        var highestPriority = ordered[0].Priority;
        var top = ordered.Where(candidate => candidate.Priority == highestPriority).ToArray();
        if (top.Length != 1)
        {
            throw new ConversionPlanningException(
                $"Conversion route {request.Source.FormatId} -> {request.TargetFormat} is ambiguous at priority {highestPriority}.");
        }

        var selected = top[0];
        return new ConversionPlan(
            SchemaVersions.Current,
            request.RequestId,
            request.Source,
            request.TargetFormat,
            selected.ProviderId,
            selected.Mode,
            request.PresetId);
    }

    private static ConversionMode? ResolveRequiredVideoMode(PlanningRequest request)
    {
        if (request.PresetId is null || !VideoPlanningPolicy.IsVideoPreset(request.PresetId))
        {
            return null;
        }

        if (request.Source.MediaFacts is null)
        {
            throw new ConversionPlanningException(
                $"Video planning rejected: {VideoPlanningReasonCode.MissingProbeFacts}.");
        }

        VideoExecutionDecision decision = VideoPlanningPolicy.Evaluate(request.PresetId, request.Source.MediaFacts);
        if (!decision.IsAllowed || decision.Mode is null)
        {
            throw new ConversionPlanningException($"Video planning rejected: {decision.ReasonCode}.");
        }

        return decision.Mode.Value;
    }
}
