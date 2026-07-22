// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for HazardIsolationSequencing aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Snapshots;

/// <summary>Immutable snapshot of a <see cref="HazardIsolationSequencing"/> aggregate for fast rehydration.</summary>
public sealed record HazardIsolationSequencingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public HazardIsolationSequencingId HazardIsolationSequencingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public HazardIsolationSequencingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}