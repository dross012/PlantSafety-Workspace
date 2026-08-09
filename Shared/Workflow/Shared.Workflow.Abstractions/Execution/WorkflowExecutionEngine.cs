// ============================================================================
//  Project:        Shared.Workflow
//  Module:         Abstractions.Execution
//  File:           WorkflowExecutionEngine.cs
//  Created:        2026-08-09
//  Description:    Phase 2.8 deliverable: the deterministic workflow
//                  execution layer. Sequences a plan's steps in order,
//                  stopping at the first failure (per stepOutcomes -- the
//                  caller-supplied, already-known result of each step;
//                  this engine has no real actuation runtime of its own,
//                  matching every other "engine" in this program, which
//                  composes/sequences deterministically and leaves actual
//                  side effects to a real executor at the boundary) and
//                  applying WorkflowRollbackPolicy to mark every already-
//                  completed step for rollback, and every not-yet-reached
//                  step as Skipped. Every step (whatever its final status)
//                  gets its own WorkflowStepEnvelope with a stable StepId
//                  and the run's shared WorkflowId/CorrelationId.
// ============================================================================
using Shared.Workflow.Abstractions.Envelope;

namespace Shared.Workflow.Abstractions.Execution;

/// <summary>Deterministically sequences a workflow plan, applying rollback rules on the first failure.</summary>
public static class WorkflowExecutionEngine
{
    /// <summary>
    /// Executes the given plan against caller-supplied per-step outcomes (stepOutcomes[i]
    /// is whether step i succeeds), applying deterministic rollback on the first failure.
    /// stepIds are generated via the given id factory, in plan order.
    /// </summary>
    public static IReadOnlyList<WorkflowStepEnvelope<TStep>> Execute<TStep>(
        IReadOnlyList<TStep> plan,
        IReadOnlyList<bool> stepOutcomes,
        Guid workflowId,
        string correlationId,
        string module,
        Func<Guid> stepIdFactory)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(stepOutcomes);
        ArgumentNullException.ThrowIfNull(stepIdFactory);
        if (plan.Count != stepOutcomes.Count)
        {
            throw new ArgumentException($"plan has {plan.Count} step(s) but stepOutcomes has {stepOutcomes.Count}; they must match 1:1.", nameof(stepOutcomes));
        }

        var statuses = new WorkflowStepStatus[plan.Count];
        int? failedIndex = null;
        for (var i = 0; i < plan.Count; i++)
        {
            if (failedIndex is not null)
            {
                statuses[i] = WorkflowStepStatus.Skipped;
                continue;
            }

            if (stepOutcomes[i])
            {
                statuses[i] = WorkflowStepStatus.Completed;
            }
            else
            {
                statuses[i] = WorkflowStepStatus.Failed;
                failedIndex = i;
            }
        }

        if (failedIndex is { } idx)
        {
            foreach (int rollbackIndex in WorkflowRollbackPolicy.DetermineRollbackIndices(plan.Count, idx))
            {
                statuses[rollbackIndex] = WorkflowStepStatus.RolledBack;
            }
        }

        var envelopes = new List<WorkflowStepEnvelope<TStep>>(plan.Count);
        for (var i = 0; i < plan.Count; i++)
        {
            envelopes.Add(new WorkflowStepEnvelope<TStep>(workflowId, stepIdFactory(), correlationId, module, statuses[i], plan[i]));
        }

        return envelopes;
    }
}
