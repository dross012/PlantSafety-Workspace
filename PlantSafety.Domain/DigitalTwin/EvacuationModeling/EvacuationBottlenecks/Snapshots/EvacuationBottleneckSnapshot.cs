// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EvacuationBottleneck aggregate rehydration. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.EvacuationBottleneck"/> aggregate for fast rehydration.</summary>
public sealed record EvacuationBottleneckSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EvacuationBottleneckId EvacuationBottleneckId { get; init; }

    /// <summary>Status at snapshot time.</summary>
    public EvacuationBottleneckStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
