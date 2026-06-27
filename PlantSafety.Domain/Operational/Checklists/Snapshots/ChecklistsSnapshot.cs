// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Checklists aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Checklists"/> aggregate for fast rehydration.</summary>
public sealed record ChecklistsSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ChecklistsId ChecklistsId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ChecklistsStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
