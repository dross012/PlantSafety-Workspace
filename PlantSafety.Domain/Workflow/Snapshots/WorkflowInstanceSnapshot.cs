// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInstanceSnapshot.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for WorkflowInstance aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Snapshots;

/// <summary>Immutable snapshot of a WorkflowInstance aggregate for fast rehydration.</summary>
public sealed record WorkflowInstanceSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public WorkflowInstanceId WorkflowInstanceId { get; init; }

    /// <summary>Identity of the originating WorkflowDefinition.</summary>
    public WorkflowDefinitionId WorkflowDefinitionId { get; init; }

    /// <summary>Overall lifecycle state at snapshot time.</summary>
    public WorkflowState State { get; init; } = WorkflowState.Default;

    /// <summary>Identity of the currently active step at snapshot time, if any.</summary>
    public WorkflowStepId? CurrentStepId { get; init; }

    /// <summary>UTC instant the instance was started.</summary>
    public UtcTimestamp StartedAtUtc { get; init; }

    /// <summary>Identity of the user who initiated the instance.</summary>
    public UserId InitiatedBy { get; init; }
}
