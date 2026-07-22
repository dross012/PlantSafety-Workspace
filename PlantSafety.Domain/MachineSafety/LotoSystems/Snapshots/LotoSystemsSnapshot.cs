// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for LotoSystems aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Snapshots;

/// <summary>Immutable snapshot of a <see cref="LotoSystems"/> aggregate for fast rehydration.</summary>
public sealed record LotoSystemsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public LotoSystemsId LotoSystemsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public LotoSystemsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}