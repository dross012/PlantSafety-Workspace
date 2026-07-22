// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PPEBehavioralMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Factories;

/// <summary>Default factory implementation for constructing <see cref="PPEBehavioralMonitoring"/> aggregates.</summary>
public sealed class PPEBehavioralMonitoringFactory : IPPEBehavioralMonitoringFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}