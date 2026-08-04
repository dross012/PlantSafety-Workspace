// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EvacuationRoute aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.EvacuationRoute"/> aggregate for fast rehydration.</summary>
public sealed record EvacuationRouteSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EvacuationRouteId EvacuationRouteId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EvacuationRouteStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
