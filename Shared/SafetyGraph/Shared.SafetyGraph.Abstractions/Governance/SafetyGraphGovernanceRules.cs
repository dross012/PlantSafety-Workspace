// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Governance
//  File:           SafetyGraphGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Pure validation over Safety Graph's
//                  contract metadata and the graph's own structural
//                  integrity, mirroring SchedulingGovernanceRules/
//                  PredictiveGovernanceRules (Phases 14/15) exactly: every
//                  rule here is a pure function -- input in, violation list
//                  out, zero side effects. ValidateGraphStructure is unique
//                  to this phase: it checks the deterministic-ordering and
//                  referential-integrity guarantees SafetyGraphAssembler is
//                  supposed to uphold, so a malformed graph (out-of-order
//                  nodes, a dangling edge reference) is caught here rather
//                  than causing a silent traversal bug downstream.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.SafetyGraph.Abstractions.Graph;

namespace Shared.SafetyGraph.Abstractions.Governance;

/// <summary>Pure validation rules for Safety Graph's contract metadata, graph structure, and escalation outcomes.</summary>
public static class SafetyGraphGovernanceRules
{
    /// <summary>Validates a SafetyGraphMetadata's shape. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateContractMetadata(SafetyGraphMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var violations = new List<string>();

        if (metadata.SchemaVersion != SafetyGraphMetadata.CurrentSchemaVersion)
        {
            violations.Add($"SchemaVersion {metadata.SchemaVersion} does not match the current SafetyGraphMetadata schema version ({SafetyGraphMetadata.CurrentSchemaVersion}).");
        }

        if (string.IsNullOrWhiteSpace(metadata.TenantId))
        {
            violations.Add("TenantId is missing or empty -- tenant propagation is broken for this contract.");
        }

        if (string.IsNullOrWhiteSpace(metadata.CorrelationId))
        {
            violations.Add("CorrelationId is missing or empty -- correlation propagation is broken for this contract.");
        }

        if (metadata.IssuedAtUtc == default)
        {
            violations.Add("IssuedAtUtc is unset (default DateTime) -- every contract must carry a real, deterministic-clock-sourced timestamp.");
        }

        return violations;
    }

    /// <summary>
    /// Validates <paramref name="graph"/>'s structural integrity: every edge's SourceNodeId/TargetNodeId
    /// must reference a real node, Nodes must be sorted by NodeId (ordinal), and Edges must be sorted by
    /// (SourceNodeId, TargetNodeId, EdgeType, EdgeId) (ordinal). Returns every violation found (empty if
    /// valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateGraphStructure(SafetyGraphModel graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var violations = new List<string>();
        var nodeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (SafetyGraphNode node in graph.Nodes)
        {
            nodeIds.Add(node.NodeId);
        }

        foreach (SafetyGraphEdge edge in graph.Edges)
        {
            if (!nodeIds.Contains(edge.SourceNodeId))
            {
                violations.Add($"Edge \"{edge.EdgeId}\" references SourceNodeId \"{edge.SourceNodeId}\", which does not exist in Nodes.");
            }

            if (!nodeIds.Contains(edge.TargetNodeId))
            {
                violations.Add($"Edge \"{edge.EdgeId}\" references TargetNodeId \"{edge.TargetNodeId}\", which does not exist in Nodes.");
            }
        }

        for (int i = 1; i < graph.Nodes.Count; i++)
        {
            if (string.CompareOrdinal(graph.Nodes[i - 1].NodeId, graph.Nodes[i].NodeId) > 0)
            {
                violations.Add("Nodes are not sorted by NodeId (ordinal) -- deterministic node ordering is broken.");
                break;
            }
        }

        for (int i = 1; i < graph.Edges.Count; i++)
        {
            SafetyGraphEdge previous = graph.Edges[i - 1];
            SafetyGraphEdge current = graph.Edges[i];
            int comparison = string.CompareOrdinal(previous.SourceNodeId, current.SourceNodeId);
            if (comparison == 0)
            {
                comparison = string.CompareOrdinal(previous.TargetNodeId, current.TargetNodeId);
            }

            if (comparison > 0)
            {
                violations.Add("Edges are not sorted by (SourceNodeId, TargetNodeId) (ordinal) -- deterministic edge ordering is broken.");
                break;
            }
        }

        return violations;
    }

    /// <summary>Validates a PredictiveEscalationOutcome's internal consistency. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateEscalationOutcome(PredictiveEscalationOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        var violations = new List<string>();

        if (outcome.RequiresEscalation && outcome.AppliedRuleCodes.Count == 0)
        {
            violations.Add("RequiresEscalation is true but AppliedRuleCodes is empty -- escalation must always be traceable to at least one applied rule.");
        }

        if (!outcome.RequiresEscalation && outcome.AppliedRuleCodes.Count > 0)
        {
            violations.Add("RequiresEscalation is false but AppliedRuleCodes is non-empty -- an applied rule must always drive escalation.");
        }

        return violations;
    }
}
