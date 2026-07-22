// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing IncidentTrackingPerformanceEvaluation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Factories;

/// <summary>Default factory implementation for constructing <see cref="IncidentTrackingPerformanceEvaluation"/> aggregates.</summary>
public sealed class IncidentTrackingPerformanceEvaluationFactory : IIncidentTrackingPerformanceEvaluationFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates.IncidentTrackingPerformanceEvaluation.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}