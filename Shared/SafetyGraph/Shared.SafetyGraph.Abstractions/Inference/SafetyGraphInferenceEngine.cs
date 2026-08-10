// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Inference
//  File:           SafetyGraphInferenceEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The public, pure entry point for a
//                  full graph inference pass: runs deterministic hazard
//                  propagation (from every Hazard node, via Causal edges),
//                  maintenance propagation (from every Machine node, via
//                  Dependency edges), and workflow propagation (from every
//                  Workflow node, via Dependency edges), detects whether any
//                  graph-wide cycle intersects each propagation's visited
//                  set, then calls SafetyGraphInferenceRules.Evaluate and
//                  PredictiveEscalationClassifier.Classify (Phase 15, reused
//                  unchanged) to produce the final SafetyGraphInferenceResult.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Traversal;

namespace Shared.SafetyGraph.Abstractions.Inference;

/// <summary>Deterministically infers graph-wide escalation from a SafetyGraph's topology and the real, upstream Phase 15 predictive result.</summary>
public static class SafetyGraphInferenceEngine
{
    /// <summary>Infers a SafetyGraphInferenceResult for <paramref name="graph"/>. Same input always produces the same output.</summary>
    public static SafetyGraphInferenceResult Infer(SafetyGraphModel graph, PredictiveSafetyEvaluationResultContract? predictiveResult)
    {
        ArgumentNullException.ThrowIfNull(graph);

        SafetyGraphTraversalResult graphCycle = SafetyGraphTraversalEngine.DetectCycle(graph);

        SafetyGraphTraversalResult hazardPropagation = Propagate(graph, SafetyGraphNodeType.Hazard, SafetyGraphEdgeType.Causal, graphCycle);
        SafetyGraphTraversalResult maintenancePropagation = Propagate(graph, SafetyGraphNodeType.Machine, SafetyGraphEdgeType.Dependency, graphCycle);
        SafetyGraphTraversalResult workflowPropagation = Propagate(graph, SafetyGraphNodeType.Workflow, SafetyGraphEdgeType.Dependency, graphCycle);

        PredictiveEscalationOutcome outcome = SafetyGraphInferenceRules.Evaluate(hazardPropagation, maintenancePropagation, workflowPropagation, predictiveResult);
        PredictivePriority priority = PredictiveEscalationClassifier.Classify(outcome);

        return new SafetyGraphInferenceResult(
            RequiresGraphWideEscalation: outcome.RequiresEscalation,
            OverallPriority: priority,
            AppliedRuleCodes: outcome.AppliedRuleCodes,
            HazardPropagationNodeIds: hazardPropagation.VisitedNodeIds,
            MaintenancePropagationNodeIds: maintenancePropagation.VisitedNodeIds,
            WorkflowPropagationNodeIds: workflowPropagation.VisitedNodeIds);
    }

    private static SafetyGraphTraversalResult Propagate(SafetyGraphModel graph, SafetyGraphNodeType startNodeType, SafetyGraphEdgeType edgeType, SafetyGraphTraversalResult graphCycle)
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency = SafetyGraphTraversalRules.BuildAdjacency(graph, edgeType);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var order = new List<string>();

        IEnumerable<SafetyGraphNode> startNodes = graph.Nodes
            .Where(node => node.NodeType == startNodeType)
            .OrderBy(node => node.NodeId, StringComparer.Ordinal);

        foreach (SafetyGraphNode startNode in startNodes)
        {
            var stack = new Stack<string>();
            stack.Push(startNode.NodeId);

            while (stack.Count > 0)
            {
                string current = stack.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                order.Add(current);

                if (adjacency.TryGetValue(current, out IReadOnlyList<string>? targets))
                {
                    for (int i = targets.Count - 1; i >= 0; i--)
                    {
                        if (!visited.Contains(targets[i]))
                        {
                            stack.Push(targets[i]);
                        }
                    }
                }
            }
        }

        bool cycleRelevant = graphCycle.CycleDetected && graphCycle.CycleNodeIds.Any(visited.Contains);

        return new SafetyGraphTraversalResult(
            order,
            Array.Empty<IReadOnlyList<string>>(),
            CycleDetected: cycleRelevant,
            CycleNodeIds: cycleRelevant ? graphCycle.CycleNodeIds : Array.Empty<string>());
    }
}
