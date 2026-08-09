// ============================================================================
//  Project:        Enterprise.WorkflowOrchestration
//  Module:         Abstractions.Orchestrator
//  File:           EnterpriseWorkflowOrchestrator.cs
//  Created:        2026-08-08
//  Description:    Phase D.6 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): coordinates the six step kinds Path D's
//                  spec requires (required actions, SOP/permit/evacuation/
//                  shutdown/restart triggers) into a single deterministic
//                  execution plan, and guards valid EnterpriseWorkflowState
//                  transitions. Pure functions, no side effects, no
//                  reflection.
//
//                  EXECUTION ORDER RULE: life-safety (Evacuation) first,
//                  then Shutdown (stop the hazard source), then the
//                  procedural gates that make further action safe/legal
//                  (Permit, then SOP), then ordinary RequiredActions, and
//                  Restart always last (nothing restarts before every
//                  other step has had the chance to run) -- the same
//                  worst-case-first precedence already used for severity/
//                  risk escalation throughout this program (see Shared.
//                  FacilityDomain's SeverityInferenceRules), applied here
//                  to step sequencing instead of classification.
// ============================================================================
using Enterprise.WorkflowOrchestration.Abstractions.Ids;
using Enterprise.WorkflowOrchestration.Abstractions.States;
using Enterprise.WorkflowOrchestration.Abstractions.Steps;
using Enterprise.WorkflowOrchestration.Abstractions.Transitions;

namespace Enterprise.WorkflowOrchestration.Abstractions.Orchestrator;

/// <summary>Coordinates enterprise workflow steps into a deterministic execution plan and guards lifecycle transitions.</summary>
public static class EnterpriseWorkflowOrchestrator
{
    private static readonly IReadOnlyDictionary<EnterpriseWorkflowStepType, int> ExecutionPriorityByStepType = new Dictionary<EnterpriseWorkflowStepType, int>
    {
        [EnterpriseWorkflowStepType.EvacuationTrigger] = 0,
        [EnterpriseWorkflowStepType.ShutdownTrigger] = 1,
        [EnterpriseWorkflowStepType.PermitTrigger] = 2,
        [EnterpriseWorkflowStepType.SopTrigger] = 3,
        [EnterpriseWorkflowStepType.RequiredAction] = 4,
        [EnterpriseWorkflowStepType.RestartTrigger] = 5,
    };

    private static readonly IReadOnlySet<(EnterpriseWorkflowState From, EnterpriseWorkflowState To)> ValidTransitions =
        new HashSet<(EnterpriseWorkflowState, EnterpriseWorkflowState)>
        {
            (EnterpriseWorkflowState.Pending, EnterpriseWorkflowState.InProgress),
            (EnterpriseWorkflowState.Pending, EnterpriseWorkflowState.Cancelled),
            (EnterpriseWorkflowState.InProgress, EnterpriseWorkflowState.Completed),
            (EnterpriseWorkflowState.InProgress, EnterpriseWorkflowState.Failed),
            (EnterpriseWorkflowState.InProgress, EnterpriseWorkflowState.Cancelled),
        };

    /// <summary>
    /// Orders the given steps into a deterministic execution plan: Evacuation, then Shutdown, then
    /// Permit, then SOP, then RequiredAction, then Restart last. Stable within each step type --
    /// steps of the same type keep their original relative order.
    /// </summary>
    public static IReadOnlyList<EnterpriseWorkflowStep> BuildExecutionPlan(IReadOnlyList<EnterpriseWorkflowStep> steps) =>
        steps.OrderBy(step => ExecutionPriorityByStepType[step.StepType]).ToArray();

    /// <summary>Produces a guarded state transition for a workflow instance.</summary>
    /// <exception cref="InvalidOperationException">The requested transition is not valid.</exception>
    public static EnterpriseWorkflowTransition Transition(EnterpriseWorkflowId workflowId, EnterpriseWorkflowState fromState, EnterpriseWorkflowState toState, DateTime nowUtc)
    {
        if (!ValidTransitions.Contains((fromState, toState)))
        {
            throw new InvalidOperationException($"Cannot transition an enterprise workflow from {fromState} to {toState}.");
        }

        return new EnterpriseWorkflowTransition(workflowId, fromState, toState, nowUtc);
    }
}
