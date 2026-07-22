// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for TwoHandControls aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Snapshots;

/// <summary>Immutable snapshot of a <see cref="TwoHandControls"/> aggregate for fast rehydration.</summary>
public sealed record TwoHandControlsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public TwoHandControlsId TwoHandControlsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public TwoHandControlsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}