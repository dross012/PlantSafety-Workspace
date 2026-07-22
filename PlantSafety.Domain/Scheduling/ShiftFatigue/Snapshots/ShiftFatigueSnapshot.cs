// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ShiftFatigue aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ShiftFatigue"/> aggregate for fast rehydration.</summary>
public sealed record ShiftFatigueSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ShiftFatigueId ShiftFatigueId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ShiftFatigueStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}