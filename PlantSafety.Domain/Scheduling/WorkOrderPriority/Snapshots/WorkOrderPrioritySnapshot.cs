// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPrioritySnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for WorkOrderPriority aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Snapshots;

/// <summary>Immutable snapshot of a <see cref="WorkOrderPriority"/> aggregate for fast rehydration.</summary>
public sealed record WorkOrderPrioritySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public WorkOrderPriorityId WorkOrderPriorityId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public WorkOrderPriorityStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}