// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for IncidentTrackingPerformanceEvaluation
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Policies;

/// <summary>Determines whether a <see cref="IncidentTrackingPerformanceEvaluation"/> aggregate is eligible for archival.</summary>
public static class IncidentTrackingPerformanceEvaluationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the evaluation record may be archived (eligibility determined).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation aggregate)
        => aggregate.Status is IncidentTrackingPerformanceEvaluationStatus.EligibilityApproved or IncidentTrackingPerformanceEvaluationStatus.EligibilityRestricted;
}
