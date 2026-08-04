// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           IEgressSimulationRunFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EgressSimulationRun aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.EgressSimulationRun"/> aggregates.</summary>
public interface IEgressSimulationRunFactory
{
    /// <summary>Starts a new EgressSimulationRun aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates.EgressSimulationRun Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, EvacuationZoneId zoneId, string twinModelVersion,
        EgressSimulationTrigger triggerReason);
}
