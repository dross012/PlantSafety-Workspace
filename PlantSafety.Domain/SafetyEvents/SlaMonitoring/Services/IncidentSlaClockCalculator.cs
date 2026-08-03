// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.SlaMonitoring
//  Layer:          Domain
//  File:           IncidentSlaClockCalculator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure domain calculation: given an incident's occurrence time, current
//                  assignment/escalation state, resolution time (if any), the current instant,
//                  and its severity's SlaPolicy, computes the remaining SLA time, an SlaStatus
//                  classification, and a deterministic recommended action. No I/O, no
//                  persistence -- computed on read by the Application-layer SLA monitor queries
//                  (UI Gaps Scoping, IncidentManagement Part 2). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.Enums;
using PlantSafety.Domain.SafetyEvents.SlaMonitoring.ValueObjects;

namespace PlantSafety.Domain.SafetyEvents.SlaMonitoring.Services;

/// <summary>Result of an <see cref="IncidentSlaClockCalculator"/> computation for a single incident.</summary>
public readonly record struct IncidentSlaClockResult(
    TimeSpan Remaining,
    SlaStatus Status,
    string RecommendedAction);

/// <summary>Pure, stateless calculator for an incident's SLA clock.</summary>
public static class IncidentSlaClockCalculator
{
    /// <summary>Fraction of the resolve-within target remaining at which an incident is classified "AtRisk".</summary>
    private const double AtRiskThresholdFraction = 0.25;

    /// <summary>Computes the SLA clock state for a single incident.</summary>
    public static IncidentSlaClockResult Compute(
        DateTimeOffset incidentOccurredAtUtc,
        bool isAssigned,
        bool isEscalated,
        bool isResolvedOrArchived,
        DateTimeOffset? resolvedAtUtc,
        DateTimeOffset nowUtc,
        SlaPolicy policy)
    {
        var effectiveNow = isResolvedOrArchived && resolvedAtUtc.HasValue ? resolvedAtUtc.Value : nowUtc;
        var elapsed = effectiveNow - incidentOccurredAtUtc;
        var remaining = policy.ResolveWithin - elapsed;

        SlaStatus status;
        if (remaining <= TimeSpan.Zero)
        {
            status = SlaStatus.Breached;
        }
        else if (remaining <= TimeSpan.FromTicks((long)(policy.ResolveWithin.Ticks * AtRiskThresholdFraction)))
        {
            status = SlaStatus.AtRisk;
        }
        else
        {
            status = SlaStatus.OnTrack;
        }

        var recommendedAction = DetermineRecommendedAction(status, isAssigned, isEscalated, isResolvedOrArchived);

        return new IncidentSlaClockResult(remaining, status, recommendedAction);
    }

    private static string DetermineRecommendedAction(SlaStatus status, bool isAssigned, bool isEscalated, bool isResolvedOrArchived)
    {
        if (isResolvedOrArchived)
            return "None";

        if (status == SlaStatus.Breached)
            return "ImmediateActionRequired";

        if (!isAssigned)
            return "AssignTechnician";

        if (status == SlaStatus.AtRisk)
            return isEscalated ? "SupervisorReview" : "Escalate";

        return "None";
    }
}
