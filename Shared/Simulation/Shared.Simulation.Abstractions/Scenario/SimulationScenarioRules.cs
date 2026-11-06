// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Scenario
//  File:           SimulationScenarioRules.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Deterministic, pure per-step
//                  escalation evaluation. Does not re-implement hazard/
//                  maintenance/routing/scheduling/predictive rule evaluation
//                  itself -- that would duplicate Phase 15's already-real,
//                  already-deterministic PredictiveSafetyEvaluationResultContract
//                  (unchanged, consumed directly). The one rule genuinely
//                  computed here is the Workflow step: it calls Shared.
//                  SafetyGraph.Abstractions.Traversal.SafetyGraphTraversalEngine.
//                  ComputeReachableNodeIds (Phase 16, unchanged) from every
//                  Workflow-typed node in the supplied graph, using the same
//                  PropagationOccurredThreshold (2) Shared.SafetyGraph.
//                  Abstractions.Inference.SafetyGraphInferenceRules already
//                  established for "propagation occurred". A null
//                  PredictiveResult/Graph deterministically means "no
//                  escalation from that source" -- never an exception, never
//                  a default-to-true.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Traversal;
using Shared.Simulation.Abstractions.Simulation;

namespace Shared.Simulation.Abstractions.Scenario;

/// <summary>The deterministic cross-module context a scenario is evaluated against.</summary>
public sealed record SimulationScenarioContext(
    SimulationScenario Scenario,
    PredictiveSafetyEvaluationResultContract? PredictiveResult,
    SafetyGraphModel? Graph);

/// <summary>Deterministic, pure per-step escalation evaluation for a simulation scenario.</summary>
public static class SimulationScenarioRules
{
    /// <summary>The minimum number of reachable nodes (from a Workflow node) that counts as "workflow propagation occurred".</summary>
    public const int WorkflowPropagationOccurredThreshold = 2;

    /// <summary>Evaluates <paramref name="step"/> deterministically against <paramref name="predictiveResult"/> and <paramref name="graph"/>. Same input always produces the same output.</summary>
    public static PredictiveEscalationOutcome EvaluateStep(
        SimulationStep step,
        PredictiveSafetyEvaluationResultContract? predictiveResult,
        SafetyGraphModel? graph)
    {
        ArgumentNullException.ThrowIfNull(step);

        var appliedRuleCodes = new List<string>();

        switch (step.StepType)
        {
            case SimulationStepType.Hazard:
                if (predictiveResult?.Hazard.RequiresImmediateEscalation == true)
                {
                    appliedRuleCodes.Add("HAZARD-STEP-ESCALATION");
                }

                break;

            case SimulationStepType.Maintenance:
                if (predictiveResult?.Maintenance.RequiresImmediateMaintenance == true)
                {
                    appliedRuleCodes.Add("MAINTENANCE-STEP-ESCALATION");
                }

                break;

            case SimulationStepType.Routing:
                if (predictiveResult?.Routing.RequiresPriorityRouting == true)
                {
                    appliedRuleCodes.Add("ROUTING-STEP-ESCALATION");
                }

                break;

            case SimulationStepType.Scheduling:
                if (predictiveResult?.Scheduling.RequiresAcceleratedScheduling == true)
                {
                    appliedRuleCodes.Add("SCHEDULING-STEP-ESCALATION");
                }

                break;

            case SimulationStepType.Workflow:
                if (graph is not null && WorkflowPropagationEscalates(graph))
                {
                    appliedRuleCodes.Add("WORKFLOW-STEP-ESCALATION");
                }

                break;

            case SimulationStepType.PredictiveInference:
                if (predictiveResult is not null && predictiveResult.OverallPriority >= PredictivePriority.High)
                {
                    appliedRuleCodes.Add("PREDICTIVE-INFERENCE-STEP-ESCALATION");
                }

                break;
        }

        return new PredictiveEscalationOutcome(appliedRuleCodes.Count > 0, appliedRuleCodes);
    }

    private static bool WorkflowPropagationEscalates(SafetyGraphModel graph)
    {
        IEnumerable<SafetyGraphNode> workflowNodes = graph.Nodes
            .Where(node => node.NodeType == SafetyGraphNodeType.Workflow)
            .OrderBy(node => node.NodeId, StringComparer.Ordinal);

        foreach (SafetyGraphNode workflowNode in workflowNodes)
        {
            IReadOnlyList<string> reachable = SafetyGraphTraversalEngine.ComputeReachableNodeIds(graph, workflowNode.NodeId);
            if (reachable.Count >= WorkflowPropagationOccurredThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
