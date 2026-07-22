// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for SafetyValvesRelays aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Snapshots;

/// <summary>Immutable snapshot of a <see cref="SafetyValvesRelays"/> aggregate for fast rehydration.</summary>
public sealed record SafetyValvesRelaysSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public SafetyValvesRelaysId SafetyValvesRelaysId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public SafetyValvesRelaysStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}