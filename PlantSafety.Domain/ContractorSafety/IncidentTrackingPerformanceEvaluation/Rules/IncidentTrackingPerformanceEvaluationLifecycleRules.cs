// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the IncidentTrackingPerformanceEvaluation
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="IncidentTrackingPerformanceEvaluation"/> aggregate.</summary>
public static class IncidentTrackingPerformanceEvaluationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when investigation may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginInvestigation(IncidentTrackingPerformanceEvaluationStatus current) => current == IncidentTrackingPerformanceEvaluationStatus.Reported;

    /// <summary>Returns <see langword="true"/> when CAPA may be documented from <paramref name="current"/>.</summary>
    public static bool CanDocumentCapa(IncidentTrackingPerformanceEvaluationStatus current) => current == IncidentTrackingPerformanceEvaluationStatus.InvestigationInProgress;

    /// <summary>Returns <see langword="true"/> when eligibility may be determined from <paramref name="current"/>.</summary>
    public static bool CanDetermineEligibility(IncidentTrackingPerformanceEvaluationStatus current) => current == IncidentTrackingPerformanceEvaluationStatus.CapaDocumented;
}
