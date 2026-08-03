// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowTransition.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Value object representing a single directed edge in a WorkflowDefinition's
//                  step graph, with an optional guard condition (Phase 5.1: "transitions";
//                  Phase 5.2: explicit state machine transitions rather than implicit strings).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.ValueObjects;

/// <summary>A single directed, optionally-guarded transition between two workflow steps.</summary>
public readonly record struct WorkflowTransition
{
    /// <summary>Source step of this transition.</summary>
    public WorkflowStepId FromStepId { get; }

    /// <summary>Destination step of this transition.</summary>
    public WorkflowStepId ToStepId { get; }

    /// <summary>Guard condition that must evaluate true (against the instance's context data) for this transition to fire.</summary>
    public GuardCondition Guard { get; }

    /// <summary>
    /// When <see langword="true"/>, this transition is the fallback taken when no guarded
    /// transition out of <see cref="FromStepId"/> is satisfied. At most one default transition is
    /// permitted per source step (enforced by the owning aggregate).
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>Creates a workflow transition.</summary>
    public WorkflowTransition(WorkflowStepId fromStepId, WorkflowStepId toStepId, GuardCondition guard, bool isDefault)
    {
        FromStepId = fromStepId;
        ToStepId = toStepId;
        Guard = guard;
        IsDefault = isDefault;
    }
}
