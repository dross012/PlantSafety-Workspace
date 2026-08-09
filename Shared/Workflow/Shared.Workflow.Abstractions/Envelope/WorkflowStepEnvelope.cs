// ============================================================================
//  Project:        Shared.Workflow
//  Module:         Abstractions.Envelope
//  File:           WorkflowStepEnvelope.cs
//  Created:        2026-08-09
//  Description:    Phase 2.8 deliverable: the workflow envelope schema.
//                  Generic over TStep so this stays independent of any one
//                  workflow-step representation (Shared.Workflow doesn't
//                  reference Enterprise.WorkflowOrchestration -- Shared.*
//                  projects sit below Enterprise.*/AI.* in this program's
//                  dependency direction, never the reverse).
// ============================================================================
namespace Shared.Workflow.Abstractions.Envelope;

/// <summary>The deterministic envelope wrapping one step within a workflow execution.</summary>
public sealed record WorkflowStepEnvelope<TStep>(
    Guid WorkflowId,
    Guid StepId,
    string CorrelationId,
    string Module,
    WorkflowStepStatus Status,
    TStep Step);
