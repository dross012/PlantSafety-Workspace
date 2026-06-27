// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for ManagementOfChange aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Snapshots;

/// <summary>Immutable snapshot of a <see cref="ManagementOfChange"/> aggregate for fast rehydration.</summary>
public sealed record ManagementOfChangeSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ManagementOfChangeId ManagementOfChangeId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ManagementOfChangeStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
