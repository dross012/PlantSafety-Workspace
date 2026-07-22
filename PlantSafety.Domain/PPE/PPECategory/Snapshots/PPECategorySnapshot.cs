// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategorySnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PPECategory aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PPECategory"/> aggregate for fast rehydration.</summary>
public sealed record PPECategorySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PPECategoryId PPECategoryId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PPECategoryStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}