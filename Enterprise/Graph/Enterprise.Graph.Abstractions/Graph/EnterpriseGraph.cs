// ============================================================================
//  Project:        Enterprise.Graph
//  Module:         Abstractions.Graph
//  File:           EnterpriseGraph.cs
//  Created:        2026-08-08
//  Description:    Phase D.1: the immutable, unified enterprise safety
//                  graph -- every node/edge is added once via
//                  EnterpriseGraphBuilder.Build(); callers read this, they
//                  don't construct or mutate it directly. Provides
//                  deterministic query helpers (by node type, outgoing/
//                  incoming edges, neighbors) rather than exposing raw
//                  LINQ-over-lists everywhere it's consumed.
// ============================================================================
using Enterprise.Graph.Abstractions.Edges;
using Enterprise.Graph.Abstractions.Enums;
using Enterprise.Graph.Abstractions.Ids;
using Enterprise.Graph.Abstractions.Nodes;

namespace Enterprise.Graph.Abstractions.Graph;

/// <summary>The immutable, unified enterprise safety graph.</summary>
public sealed record EnterpriseGraph(
    IReadOnlyList<EnterpriseGraphNode> Nodes,
    IReadOnlyList<EnterpriseGraphEdge> Edges)
{
    /// <summary>Every node of the given type, in the order they were added.</summary>
    public IReadOnlyList<EnterpriseGraphNode> NodesOfType(EnterpriseNodeType nodeType) =>
        Nodes.Where(n => n.NodeType == nodeType).ToArray();

    /// <summary>Every edge originating from the given node.</summary>
    public IReadOnlyList<EnterpriseGraphEdge> EdgesFrom(EnterpriseNodeId nodeId) =>
        Edges.Where(e => e.FromNodeId == nodeId).ToArray();

    /// <summary>Every edge terminating at the given node.</summary>
    public IReadOnlyList<EnterpriseGraphEdge> EdgesTo(EnterpriseNodeId nodeId) =>
        Edges.Where(e => e.ToNodeId == nodeId).ToArray();

    /// <summary>Every node directly reachable from the given node via an outgoing edge, optionally filtered to one edge type.</summary>
    public IReadOnlyList<EnterpriseGraphNode> Neighbors(EnterpriseNodeId nodeId, EnterpriseEdgeType? edgeType = null)
    {
        IEnumerable<EnterpriseNodeId> targetIds = Edges
            .Where(e => e.FromNodeId == nodeId && (edgeType is null || e.EdgeType == edgeType))
            .Select(e => e.ToNodeId);

        var targetIdSet = targetIds.ToHashSet();
        return Nodes.Where(n => targetIdSet.Contains(n.NodeId)).ToArray();
    }
}
