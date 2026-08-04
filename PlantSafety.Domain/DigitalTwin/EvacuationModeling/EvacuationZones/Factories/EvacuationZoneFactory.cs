// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EvacuationZone aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.EvacuationZone"/> aggregates.</summary>
public sealed class EvacuationZoneFactory : Interfaces.IEvacuationZoneFactory
{
    /// <inheritdoc/>
    public Aggregates.EvacuationZone Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string zoneName, string twinModelVersion, int zoneCapacity, int occupancyThreshold)
        => Aggregates.EvacuationZone.Create(createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, zoneName, twinModelVersion, zoneCapacity, occupancyThreshold);
}
