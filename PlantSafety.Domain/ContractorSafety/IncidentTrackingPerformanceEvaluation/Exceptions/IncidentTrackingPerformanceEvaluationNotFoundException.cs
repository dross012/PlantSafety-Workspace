// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a IncidentTrackingPerformanceEvaluation aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Exceptions;

/// <summary>Thrown when a <see cref="IncidentTrackingPerformanceEvaluation"/> aggregate lookup finds no match.</summary>
public sealed class IncidentTrackingPerformanceEvaluationNotFoundException : IncidentTrackingPerformanceEvaluationDomainException
{
    /// <summary>Creates a not-found exception for the given IncidentTrackingPerformanceEvaluation identity.</summary>
    public IncidentTrackingPerformanceEvaluationNotFoundException(IncidentTrackingPerformanceEvaluationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "IncidentTrackingPerformanceEvaluation with id '{id}' was not found.",
            "IncidentTrackingPerformanceEvaluation.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}