// ============================================================================
//  Project:        Enterprise.WorkflowOrchestration
//  Module:         Abstractions.States
//  File:           EnterpriseWorkflowState.cs
//  Created:        2026-08-08
//  Description:    Phase D.6: the lifecycle state of an orchestrated
//                  enterprise workflow instance. Valid transitions are
//                  enforced by EnterpriseWorkflowOrchestrator.Transition,
//                  not by this enum itself. Additive-only: new members may
//                  be appended, existing members must never be renumbered
//                  or removed once published.
// ============================================================================
namespace Enterprise.WorkflowOrchestration.Abstractions.States;

/// <summary>The lifecycle state of an orchestrated enterprise workflow instance.</summary>
public enum EnterpriseWorkflowState
{
    /// <summary>Created but not yet started.</summary>
    Pending,

    /// <summary>Actively executing its coordinated steps.</summary>
    InProgress,

    /// <summary>Every coordinated step completed successfully. Terminal.</summary>
    Completed,

    /// <summary>Execution stopped due to a failure. Terminal.</summary>
    Failed,

    /// <summary>Execution was cancelled before completion. Terminal.</summary>
    Cancelled,
}
