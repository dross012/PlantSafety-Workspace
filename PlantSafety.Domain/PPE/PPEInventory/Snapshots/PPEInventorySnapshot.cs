// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventorySnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PPEInventory aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PPEInventory"/> aggregate for fast rehydration.</summary>
public sealed record PPEInventorySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PPEInventoryId PPEInventoryId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PPEInventoryStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}