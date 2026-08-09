// ============================================================================
//  Project:        AI.EnterpriseOrchestration
//  Module:         Abstractions.Engine
//  File:           EnterpriseOrchestrationEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.9 (Path E: AI-Driven Safety Automation): the
//                  final composition phase -- aggregates every already-
//                  computed per-facility Path E engine output (Workflow,
//                  Hazard, Regulatory, Predictive, digital-twin severity)
//                  across the whole enterprise. No hazard/workflow/
//                  regulatory/predictive/severity logic is reimplemented
//                  here, only cross-facility aggregation. Pure function,
//                  no side effects, no reflection.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

using AI.EnterpriseOrchestration.Abstractions.Orchestration;
using AI.EnterpriseOrchestration.Abstractions.Rules;

namespace AI.EnterpriseOrchestration.Abstractions.Engine;

/// <summary>Aggregates Path E's per-facility engine outputs across the whole enterprise.</summary>
public static class EnterpriseOrchestrationEngine
{
    /// <summary>Orchestrates across every facility in the input and returns the aggregated enterprise output.</summary>
    public static EnterpriseOrchestrationOutput Orchestrate(EnterpriseOrchestrationInput input, IReadOnlyList<EnterpriseOrchestrationRule> rules, DateTime? nowUtc = null)
    {
        IReadOnlyList<Guid> facilitiesRequiringEvacuation = input.Facilities
            .Where(f => f.Workflow.EvacuationSteps.Count > 0)
            .Select(f => f.FacilityId)
            .ToArray();

        IReadOnlyList<Guid> facilitiesWithRegulatoryExposure = input.Facilities
            .Where(f => f.Regulatory.Interpretations.Count > 0)
            .Select(f => f.FacilityId)
            .ToArray();

        IReadOnlyList<Guid> facilitiesWithAnomalousPredictions = input.Facilities
            .Where(f => f.Predictive.Models.Any(m => m.IsAnomalous))
            .Select(f => f.FacilityId)
            .ToArray();

        SeverityLevel highestSeverity = input.Facilities.Count == 0
            ? SeverityLevel.Minor
            : input.Facilities.Select(f => f.DigitalTwinSimulatedSeverity).Max();

        IReadOnlyList<string> alerts = rules
            .Where(rule => rule.Condition(input.Facilities))
            .Select(rule => rule.Alert)
            .ToArray();

        return new EnterpriseOrchestrationOutput(
            input.Facilities.Count,
            facilitiesRequiringEvacuation,
            facilitiesWithRegulatoryExposure,
            facilitiesWithAnomalousPredictions,
            highestSeverity,
            alerts,
            nowUtc ?? DateTime.UtcNow);
    }
}
