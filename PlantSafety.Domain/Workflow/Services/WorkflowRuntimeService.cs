// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowRuntimeService.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service coordinating the deterministic workflow runtime engine across
//                  the WorkflowDefinition (step graph + guarded transitions) and WorkflowInstance
//                  (runtime state machine) aggregates -- logic that cannot live on either aggregate
//                  alone because it requires both. Domain services must not reference
//                  infrastructure (no HTTP/SQL/logging); all timestamps are supplied by the caller.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Policies;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Services;

/// <summary>
/// Domain service that advances a WorkflowInstance through its WorkflowDefinition's guarded step
/// graph, and evaluates timeout/escalation for the instance's currently active step.
/// </summary>
public class WorkflowRuntimeService : IDomainService
{
    /// <summary>
    /// Resolves and activates the next step out of <paramref name="instance"/>'s current step,
    /// according to <paramref name="definition"/>'s guarded transition graph and the instance's
    /// context data. When no outgoing transition resolves, the instance is marked Completed.
    /// </summary>
    public void AdvanceToNextStep(
        WorkflowDefinition definition,
        WorkflowInstance instance,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (instance.CurrentStepId is not { } currentStepId)
            throw new InvalidOperationException($"WorkflowInstance {instance.WorkflowInstanceId} has no current step to advance from.");

        instance.CompleteCurrentStep(nowUtc);

        var nextStep = definition.ResolveNextStep(currentStepId, instance.ContextData);
        if (nextStep is null)
        {
            instance.Complete(nowUtc, actor, correlationId, causationId);
            return;
        }

        if (!nextStep.EntryCondition.Evaluate(instance.ContextData))
        {
            // The resolved step's own entry guard is not satisfied: treat the workflow as
            // complete rather than activating an inapplicable step. Callers that require a
            // stricter contract can inspect ResolveNextStep separately before calling this method.
            instance.Complete(nowUtc, actor, correlationId, causationId);
            return;
        }

        instance.ActivateStep(
            nextStep.StepId, nextStep.StepType, nextStep.AssignedActor, currentStepId, nowUtc, actor, correlationId, causationId);
    }

    /// <summary>
    /// Evaluates whether the instance's currently active step has exceeded its configured timeout
    /// window and, if so, marks it timed out and escalates it to the step definition's resolved
    /// escalation target.
    /// </summary>
    public bool EvaluateTimeoutAndEscalate(
        WorkflowDefinition definition,
        WorkflowInstance instance,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (instance.CurrentStepId is not { } currentStepId)
            return false;

        var stepDefinition = definition.FindStep(currentStepId)
            ?? throw new Exceptions.WorkflowStepNotFoundException(currentStepId, correlationId, causationId);

        var currentStepInstance = instance.StepHistory.LastOrDefault(s => s.StepId == currentStepId);
        if (currentStepInstance is null)
            return false;

        if (!WorkflowTimeoutPolicyEvaluator.HasTimedOut(currentStepInstance, stepDefinition.Timeout, nowUtc))
            return false;

        instance.TimeoutCurrentStep(nowUtc, actor, correlationId, causationId);

        if (stepDefinition.Timeout!.Value.Action == Enums.TimeoutAction.Escalate)
        {
            var escalationTarget = WorkflowEscalationPolicy.ResolveEscalationTarget(stepDefinition);
            instance.EscalateCurrentStep(escalationTarget, nowUtc, actor, correlationId, causationId);
        }

        return true;
    }
}
