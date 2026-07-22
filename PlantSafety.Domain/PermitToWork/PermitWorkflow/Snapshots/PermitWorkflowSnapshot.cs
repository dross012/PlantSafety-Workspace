// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PermitWorkflow aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PermitWorkflow"/> aggregate for fast rehydration.</summary>
public sealed record PermitWorkflowSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PermitWorkflowId PermitWorkflowId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PermitWorkflowStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}