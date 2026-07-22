// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IIncidentTrackingPerformanceEvaluationFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing IncidentTrackingPerformanceEvaluation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Interfaces;

/// <summary>Factory interface for constructing <see cref="IncidentTrackingPerformanceEvaluation"/> aggregates.</summary>
public interface IIncidentTrackingPerformanceEvaluationFactory
{
    /// <summary>Creates a new IncidentTrackingPerformanceEvaluation aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}