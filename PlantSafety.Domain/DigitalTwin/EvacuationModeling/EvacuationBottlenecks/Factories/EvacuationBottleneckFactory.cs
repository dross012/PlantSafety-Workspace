// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EvacuationBottleneck
//                  aggregates. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.EvacuationBottleneck"/> aggregates.</summary>
public sealed class EvacuationBottleneckFactory : Interfaces.IEvacuationBottleneckFactory
{
    /// <inheritdoc/>
    public Aggregates.EvacuationBottleneck Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, EvacuationRouteId routeId, EgressSimulationRunId simulationRunId,
        string bottleneckLocation, int delaySeconds)
        => Aggregates.EvacuationBottleneck.Create(createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, routeId, simulationRunId, bottleneckLocation, delaySeconds);
}
