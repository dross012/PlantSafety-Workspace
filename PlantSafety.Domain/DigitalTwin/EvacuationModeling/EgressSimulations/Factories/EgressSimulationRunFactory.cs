// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EgressSimulationRun aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.EgressSimulationRun"/> aggregates.</summary>
public sealed class EgressSimulationRunFactory : Interfaces.IEgressSimulationRunFactory
{
    /// <inheritdoc/>
    public Aggregates.EgressSimulationRun Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, EvacuationZoneId zoneId, string twinModelVersion,
        EgressSimulationTrigger triggerReason)
        => Aggregates.EgressSimulationRun.Create(createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, zoneId, twinModelVersion, triggerReason);
}
