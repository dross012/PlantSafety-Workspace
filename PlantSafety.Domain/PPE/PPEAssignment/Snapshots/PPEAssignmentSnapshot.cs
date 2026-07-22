// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PPEAssignment aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PPEAssignment"/> aggregate for fast rehydration.</summary>
public sealed record PPEAssignmentSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PPEAssignmentId PPEAssignmentId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PPEAssignmentStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}