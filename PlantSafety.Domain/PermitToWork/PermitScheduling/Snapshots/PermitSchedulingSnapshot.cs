// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PermitScheduling aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PermitScheduling"/> aggregate for fast rehydration.</summary>
public sealed record PermitSchedulingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PermitSchedulingId PermitSchedulingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PermitSchedulingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}