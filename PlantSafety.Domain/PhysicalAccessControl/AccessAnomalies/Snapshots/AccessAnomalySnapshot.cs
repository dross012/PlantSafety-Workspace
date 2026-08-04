// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalySnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AccessAnomaly aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Snapshots;

/// <summary>Immutable snapshot of an <see cref="AccessAnomaly"/> aggregate for fast rehydration.</summary>
public sealed record AccessAnomalySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AccessAnomalyId AccessAnomalyId { get; init; }

    /// <summary>Triage status at snapshot time.</summary>
    public AccessAnomalyStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user/system actor who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
