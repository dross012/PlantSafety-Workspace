// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepDefinition.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of the WorkflowDefinition aggregate describing a single step:
//                  its resolution mode, assigned actor, entry guard condition, timeout, and retry
//                  policy (Phase 5.1 workflow definitions: "steps ... assigned roles/actors,
//                  conditions"; Phase 5.2 runtime: timeout/retry rules).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Entities;

/// <summary>A single step definition within a WorkflowDefinition's step graph.</summary>
public sealed class WorkflowStepDefinition
{
    /// <summary>Unique identity of this step within its owning WorkflowDefinition.</summary>
    public WorkflowStepId StepId { get; private set; }

    /// <summary>Identity of the owning WorkflowDefinition aggregate.</summary>
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }

    /// <summary>1-based ordinal position of this step, used to determine the graph's start step.</summary>
    public int Order { get; private set; }

    /// <summary>Human-readable step name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>How this step is resolved (human approval, automated decision, predictive recommendation, notification, escalation).</summary>
    public WorkflowStepType StepType { get; private set; } = WorkflowStepType.Default;

    /// <summary>Role or specific user assigned to resolve this step.</summary>
    public AssignedActor AssignedActor { get; private set; }

    /// <summary>Escalation target used when this step times out with <see cref="Enums.TimeoutAction.Escalate"/>.</summary>
    public AssignedActor EscalationActor { get; private set; }

    /// <summary>Guard condition evaluated against the instance's context data before this step may be entered.</summary>
    public GuardCondition EntryCondition { get; private set; } = GuardCondition.None;

    /// <summary>Timeout policy for this step, or <see langword="null"/> when the step never times out.</summary>
    public TimeoutPolicy? Timeout { get; private set; }

    /// <summary>Retry policy for this step, or <see langword="null"/> when the step is never retried.</summary>
    public RetryPolicy? Retry { get; private set; }

    /// <summary>For ORM/reflection rehydration only.</summary>
    private WorkflowStepDefinition() { }

    /// <summary>Creates a new workflow step definition.</summary>
    public WorkflowStepDefinition(
        WorkflowDefinitionId workflowDefinitionId,
        int order,
        string name,
        WorkflowStepType stepType,
        AssignedActor assignedActor,
        AssignedActor escalationActor,
        GuardCondition entryCondition,
        TimeoutPolicy? timeout,
        RetryPolicy? retry)
    {
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), order, "Step order must be >= 1.");

        StepId = WorkflowStepId.New();
        WorkflowDefinitionId = workflowDefinitionId;
        Order = order;
        Name = (name ?? string.Empty).Trim();
        StepType = stepType;
        AssignedActor = assignedActor;
        EscalationActor = escalationActor;
        EntryCondition = entryCondition;
        Timeout = timeout;
        Retry = retry;
    }
}
