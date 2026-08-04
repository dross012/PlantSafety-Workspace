// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           IEvacuationBottleneckFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EvacuationBottleneck aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.EvacuationBottleneck"/> aggregates.</summary>
public interface IEvacuationBottleneckFactory
{
    /// <summary>Records a newly detected EvacuationBottleneck aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates.EvacuationBottleneck Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, EvacuationRouteId routeId, EgressSimulationRunId simulationRunId,
        string bottleneckLocation, int delaySeconds);
}
