// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           TimeoutAction.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Action taken when a WorkflowStepInstance exceeds its configured timeout window
//                  (Phase 5.2 timeout rules).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Workflow.Enums;

/// <summary>Action taken when a workflow step exceeds its configured timeout window.</summary>
public enum TimeoutAction
{
    /// <summary>Escalate the step to its designated escalation actor.</summary>
    Escalate = 0,

    /// <summary>Fail the step (and, if no retry budget remains, the instance).</summary>
    Fail = 1,

    /// <summary>Schedule a bounded retry of the step.</summary>
    Retry = 2,
}
