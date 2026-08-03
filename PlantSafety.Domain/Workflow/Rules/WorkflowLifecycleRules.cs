// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines the explicit, permitted lifecycle transitions for the WorkflowInstance
//                  state machine (Phase 5.2: "explicit states + allowed transitions, not implicit
//                  strings").
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Workflow.Enums;

namespace PlantSafety.Domain.Workflow.Rules;

/// <summary>Defines permitted lifecycle transitions for the WorkflowInstance state machine.</summary>
public static class WorkflowLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a human approval decision may be recorded from <paramref name="current"/>.</summary>
    public static bool CanApproveOrReject(WorkflowState current)
        => current is not null && !current.IsTerminal && current.Code is "WaitingForApproval" or "InProgress" or "Retrying";

    /// <summary>Returns <see langword="true"/> when an automated decision may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordAutomatedDecision(WorkflowState current)
        => current is not null && !current.IsTerminal;

    /// <summary>Returns <see langword="true"/> when the instance may be escalated from <paramref name="current"/>.</summary>
    public static bool CanEscalate(WorkflowState current)
        => current is not null && current.Code is "TimedOut" or "InProgress" or "WaitingForApproval";

    /// <summary>Returns <see langword="true"/> when the instance may schedule a retry from <paramref name="current"/>.</summary>
    public static bool CanRetry(WorkflowState current)
        => current is not null && current.Code is "TimedOut" or "InProgress" or "WaitingForApproval" or "Escalated";

    /// <summary>Returns <see langword="true"/> when the instance may transition to Completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(WorkflowState current)
        => current is not null && !current.IsTerminal;

    /// <summary>Returns <see langword="true"/> when the instance may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(WorkflowState current)
        => current is not null && !current.IsTerminal;
}
