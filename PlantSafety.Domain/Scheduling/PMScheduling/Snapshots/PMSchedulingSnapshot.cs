// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PMScheduling aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PMScheduling"/> aggregate for fast rehydration.</summary>
public sealed record PMSchedulingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PMSchedulingId PMSchedulingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PMSchedulingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}