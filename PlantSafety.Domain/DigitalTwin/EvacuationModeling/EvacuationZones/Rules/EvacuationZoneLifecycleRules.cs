// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions and freshness rules for the
//                  EvacuationZone aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.EvacuationZone"/> aggregate.</summary>
public static class EvacuationZoneLifecycleRules
{
    /// <summary>Maximum age of an occupancy reading before it is considered stale for the freshness KPI.</summary>
    public static readonly TimeSpan OccupancyFreshnessWindow = TimeSpan.FromMinutes(15);

    /// <summary>Returns <see langword="true"/> when an occupancy snapshot may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordOccupancy(EvacuationZoneStatus current) => current == EvacuationZoneStatus.Active;

    /// <summary>Returns <see langword="true"/> when the zone may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(EvacuationZoneStatus current) => current != EvacuationZoneStatus.Retired;

    /// <summary>Returns <see langword="true"/> when <paramref name="lastReadingAtUtc"/> is still within the freshness window relative to <paramref name="nowUtc"/>.</summary>
    public static bool IsOccupancyReadingFresh(UtcTimestamp? lastReadingAtUtc, UtcTimestamp nowUtc) =>
        lastReadingAtUtc is not null && nowUtc.Value - lastReadingAtUtc.Value.Value <= OccupancyFreshnessWindow;
}
