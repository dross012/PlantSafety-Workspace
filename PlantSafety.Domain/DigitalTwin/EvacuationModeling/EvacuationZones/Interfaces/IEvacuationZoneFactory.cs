// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           IEvacuationZoneFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EvacuationZone aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.EvacuationZone"/> aggregates.</summary>
public interface IEvacuationZoneFactory
{
    /// <summary>Defines a new EvacuationZone aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates.EvacuationZone Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string zoneName, string twinModelVersion, int zoneCapacity, int occupancyThreshold);
}
