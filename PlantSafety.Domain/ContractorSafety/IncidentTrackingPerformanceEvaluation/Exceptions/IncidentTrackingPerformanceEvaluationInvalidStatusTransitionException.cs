// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a IncidentTrackingPerformanceEvaluation lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Exceptions;

/// <summary>Thrown when a <see cref="IncidentTrackingPerformanceEvaluation"/> lifecycle transition is not permitted.</summary>
public sealed class IncidentTrackingPerformanceEvaluationInvalidStatusTransitionException : IncidentTrackingPerformanceEvaluationDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public IncidentTrackingPerformanceEvaluationInvalidStatusTransitionException(
        IncidentTrackingPerformanceEvaluationId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'IncidentTrackingPerformanceEvaluation' {id} from '{from}' to '{to}'.",
            "IncidentTrackingPerformanceEvaluation.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}