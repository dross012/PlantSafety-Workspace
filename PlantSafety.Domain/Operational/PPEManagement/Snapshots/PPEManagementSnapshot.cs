// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PPEManagement aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PPEManagement"/> aggregate for fast rehydration.</summary>
public sealed record PPEManagementSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PPEManagementId PPEManagementId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PPEManagementStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
