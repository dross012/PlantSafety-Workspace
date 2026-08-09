// ============================================================================
//  Project:        Enterprise.WorkflowOrchestration
//  Module:         Abstractions.Transitions
//  File:           EnterpriseWorkflowTransition.cs
//  Created:        2026-08-08
//  Description:    Phase D.6: a single lifecycle-state transition of an
//                  orchestrated enterprise workflow instance. Produced
//                  exclusively by EnterpriseWorkflowOrchestrator.Transition,
//                  which guards which FromState -> ToState pairs are valid.
// ============================================================================
using Enterprise.WorkflowOrchestration.Abstractions.Ids;
using Enterprise.WorkflowOrchestration.Abstractions.States;

namespace Enterprise.WorkflowOrchestration.Abstractions.Transitions;

/// <summary>A single lifecycle-state transition of an orchestrated enterprise workflow instance.</summary>
public sealed record EnterpriseWorkflowTransition(
    EnterpriseWorkflowId WorkflowId,
    EnterpriseWorkflowState FromState,
    EnterpriseWorkflowState ToState,
    DateTime OccurredAtUtc);
