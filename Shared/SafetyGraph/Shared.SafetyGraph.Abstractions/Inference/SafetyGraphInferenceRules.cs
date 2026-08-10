// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Inference
//  File:           SafetyGraphInferenceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministic, pure escalation
//                  evaluation over a graph's already-computed propagation
//                  results (hazard/maintenance/workflow, via
//                  SafetyGraphTraversalEngine) plus the real, upstream
//                  PredictiveSafetyEvaluationResultContract (Phase 15,
//                  unchanged). Reuses Shared.PredictiveSafety.Abstractions.
//                  Common.PredictiveEscalationOutcome as its return shape --
//                  the same additive-accumulation, worst-case-wins contract
//                  every Phase 15 rule engine already uses -- rather than
//                  inventing a parallel one. This engine does not
//                  re-implement hazard/maintenance/routing/scheduling/audit
//                  rule evaluation itself (that would duplicate Phase 15's
//                  already-real, already-deterministic logic); it evaluates
//                  what the graph's TOPOLOGY adds on top: how far each kind
//                  of decision propagates once placed in the graph.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Traversal;

namespace Shared.SafetyGraph.Abstractions.Inference;

/// <summary>Deterministic, pure escalation evaluation over a graph's propagation and predictive results.</summary>
public static class SafetyGraphInferenceRules
{
    /// <summary>The minimum number of visited nodes (including the start node) that counts as "propagation occurred".</summary>
    public const int PropagationOccurredThreshold = 2;

    /// <summary>
    /// Evaluates <paramref name="hazardPropagation"/>, <paramref name="maintenancePropagation"/>,
    /// <paramref name="workflowPropagation"/>, and <paramref name="predictiveResult"/> deterministically.
    /// Same input always produces the same output.
    /// </summary>
    public static PredictiveEscalationOutcome Evaluate(
        SafetyGraphTraversalResult hazardPropagation,
        SafetyGraphTraversalResult maintenancePropagation,
        SafetyGraphTraversalResult workflowPropagation,
        PredictiveSafetyEvaluationResultContract? predictiveResult)
    {
        ArgumentNullException.ThrowIfNull(hazardPropagation);
        ArgumentNullException.ThrowIfNull(maintenancePropagation);
        ArgumentNullException.ThrowIfNull(workflowPropagation);

        var appliedRuleCodes = new List<string>();

        if (hazardPropagation.CycleDetected)
        {
            appliedRuleCodes.Add("HAZARD-CYCLE-ESCALATION");
        }

        if (hazardPropagation.VisitedNodeIds.Count >= PropagationOccurredThreshold)
        {
            appliedRuleCodes.Add("HAZARD-PROPAGATION-ESCALATION");
        }

        if (maintenancePropagation.VisitedNodeIds.Count >= PropagationOccurredThreshold)
        {
            appliedRuleCodes.Add("MAINTENANCE-PROPAGATION-ESCALATION");
        }

        if (workflowPropagation.VisitedNodeIds.Count >= PropagationOccurredThreshold)
        {
            appliedRuleCodes.Add("WORKFLOW-PROPAGATION-ESCALATION");
        }

        if (predictiveResult is not null && predictiveResult.OverallPriority >= PredictivePriority.High)
        {
            appliedRuleCodes.Add("PREDICTIVE-SAFETY-ESCALATION");
        }

        return new PredictiveEscalationOutcome(appliedRuleCodes.Count > 0, appliedRuleCodes);
    }
}
