// ============================================================================
//  Project:        Shared.Workflow
//  Module:         Abstractions.Envelope
//  File:           WorkflowStepStatus.cs
//  Created:        2026-08-09
//  Description:    Phase 2.8 (WorkflowEngine Stabilization).
// ============================================================================
namespace Shared.Workflow.Abstractions.Envelope;

/// <summary>The deterministic outcome of one workflow step within an execution.</summary>
public enum WorkflowStepStatus
{
    /// <summary>Not yet reached.</summary>
    Pending,

    /// <summary>Completed successfully.</summary>
    Completed,

    /// <summary>Failed; triggered rollback of the steps before it.</summary>
    Failed,

    /// <summary>Already-completed and reversed because a later step failed.</summary>
    RolledBack,

    /// <summary>Never attempted because an earlier step failed.</summary>
    Skipped,
}
