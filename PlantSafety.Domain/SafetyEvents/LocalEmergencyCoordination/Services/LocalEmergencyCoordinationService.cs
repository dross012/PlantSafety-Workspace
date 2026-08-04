// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for LocalEmergencyCoordination. Coordinates cross-entity logic
//                  (e.g. joint debrief completion rate) that cannot live on a single aggregate
//                  instance. Domain services must not reference infrastructure (no HTTP/SQL/logging).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Services;

/// <summary>Domain service for LocalEmergencyCoordination. Coordinates logic that cannot live on a single aggregate.</summary>
public sealed class LocalEmergencyCoordinationService : IDomainService
{
    /// <summary>
    /// Computes the joint debrief completion rate KPI: the proportion of closed/completed joint
    /// response plans for which at least one <see cref="Entities.JointDebriefRecord"/> has been
    /// recorded referencing the same incident.
    /// </summary>
    public double ComputeJointDebriefCompletionRate(
        IReadOnlyCollection<Entities.JointResponsePlan> plans,
        IReadOnlyCollection<Entities.JointDebriefRecord> debriefs)
    {
        var concludedPlans = plans.Where(p => !p.IsActive).ToList();
        if (concludedPlans.Count == 0)
            return 1.0;

        var debriefedIncidentIds = debriefs
            .Where(d => d.IncidentId.HasValue)
            .Select(d => d.IncidentId!.Value)
            .ToHashSet();

        var completedCount = concludedPlans.Count(p => p.IncidentId.HasValue && debriefedIncidentIds.Contains(p.IncidentId!.Value));

        return (double)completedCount / concludedPlans.Count;
    }
}
