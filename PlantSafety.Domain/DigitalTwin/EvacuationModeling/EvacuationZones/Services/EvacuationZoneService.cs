// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain service for EvacuationZone. Coordinates logic that cannot live on a
//                  single aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Services;

/// <summary>Domain service for EvacuationZone. Coordinates logic that cannot live on a single aggregate.</summary>
public class EvacuationZoneService : IDomainService
{
    /// <summary>
    /// Computes the occupancy utilization ratio (0.0-1.0+) of a zone's most recent reading against
    /// its rated capacity, for use by the Live Evacuation Readiness Board dashboard.
    /// Domain services must not reference infrastructure (no HTTP/SQL/logging).
    /// </summary>
    public decimal ComputeUtilizationRatio(Aggregates.EvacuationZone zone)
        => zone.ZoneCapacity <= 0 ? 0m : Math.Round((decimal)zone.CurrentOccupancyCount / zone.ZoneCapacity, 4);
}
