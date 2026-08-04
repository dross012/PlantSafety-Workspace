// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           RouteObstructionEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable entity representing a single obstruction record (open or cleared) on
//                  an EvacuationRoute aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Entities;

/// <summary>Records a single obstruction (open or cleared) on an <see cref="EvacuationRoute"/> aggregate.</summary>
public sealed record RouteObstructionEntry
{
    /// <summary>Identity of this obstruction record.</summary>
    public RouteObstructionId RouteObstructionId { get; }

    /// <summary>Identity of the parent EvacuationRoute aggregate.</summary>
    public EvacuationRouteId EvacuationRouteId { get; }

    /// <summary>Free-text description of the obstruction (e.g. blocked door, debris, BMS-reported fire door fault).</summary>
    public string Description { get; }

    /// <summary>Severity classification of the obstruction.</summary>
    public ObstructionSeverity Severity { get; }

    /// <summary>UTC timestamp the obstruction was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; }

    /// <summary>UTC timestamp the obstruction was cleared, or <see langword="null"/> while still open.</summary>
    public UtcTimestamp? ClearedAtUtc { get; init; }

    /// <summary>Creates a new obstruction record.</summary>
    public RouteObstructionEntry(
        RouteObstructionId routeObstructionId, EvacuationRouteId evacuationRouteId,
        string description, ObstructionSeverity severity,
        UtcTimestamp recordedAtUtc, UtcTimestamp? clearedAtUtc)
    {
        RouteObstructionId = routeObstructionId;
        EvacuationRouteId  = evacuationRouteId;
        Description        = description;
        Severity           = severity;
        RecordedAtUtc      = recordedAtUtc;
        ClearedAtUtc        = clearedAtUtc;
    }

    /// <summary>Returns a copy of this entry marked as cleared at <paramref name="clearedAtUtc"/>.</summary>
    public RouteObstructionEntry WithCleared(UtcTimestamp clearedAtUtc) =>
        this with { ClearedAtUtc = clearedAtUtc };
}
