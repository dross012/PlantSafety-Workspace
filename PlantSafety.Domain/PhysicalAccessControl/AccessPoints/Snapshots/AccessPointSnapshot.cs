// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AccessPoint aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Snapshots;

/// <summary>Immutable snapshot of an <see cref="AccessPoint"/> aggregate for fast rehydration.</summary>
public sealed record AccessPointSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AccessPointId AccessPointId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AccessPointStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
