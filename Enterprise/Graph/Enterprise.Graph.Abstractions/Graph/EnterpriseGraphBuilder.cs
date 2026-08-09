// ============================================================================
//  Project:        Enterprise.Graph
//  Module:         Abstractions.Graph
//  File:           EnterpriseGraphBuilder.cs
//  Created:        2026-08-08
//  Description:    Phase D.1: a fluent, mutable builder for an immutable
//                  EnterpriseGraph -- the only supported way to construct
//                  one. Deterministic: nodes/edges are appended in exactly
//                  the order AddNode/AddEdge is called. Rejects edges
//                  referencing a node id that hasn't been added yet
//                  (dangling-edge guard) and duplicate node ids.
// ============================================================================
using Enterprise.Graph.Abstractions.Edges;
using Enterprise.Graph.Abstractions.Enums;
using Enterprise.Graph.Abstractions.Ids;
using Enterprise.Graph.Abstractions.Nodes;

namespace Enterprise.Graph.Abstractions.Graph;

/// <summary>A fluent, mutable builder for an immutable <see cref="EnterpriseGraph"/>.</summary>
public sealed class EnterpriseGraphBuilder
{
    private readonly List<EnterpriseGraphNode> _nodes = [];
    private readonly List<EnterpriseGraphEdge> _edges = [];
    private readonly HashSet<Guid> _nodeIds = [];

    /// <summary>Adds a node to the graph being built. Returns this builder for fluent chaining.</summary>
    /// <exception cref="ArgumentException"><paramref name="label"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">A node with this <paramref name="nodeId"/> already exists.</exception>
    public EnterpriseGraphBuilder AddNode(EnterpriseNodeId nodeId, EnterpriseNodeType nodeType, Guid sourceId, string label, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Node label is required.", nameof(label));
        }
        if (!_nodeIds.Add(nodeId.Value))
        {
            throw new InvalidOperationException($"A node with id '{nodeId}' has already been added.");
        }

        _nodes.Add(new EnterpriseGraphNode(nodeId, nodeType, sourceId, label, nowUtc));
        return this;
    }

    /// <summary>Adds a directed edge to the graph being built. Returns this builder for fluent chaining.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="fromNodeId"/> or <paramref name="toNodeId"/> does not reference an already-added node.</exception>
    public EnterpriseGraphBuilder AddEdge(EnterpriseNodeId fromNodeId, EnterpriseNodeId toNodeId, EnterpriseEdgeType edgeType, DateTime nowUtc)
    {
        if (!_nodeIds.Contains(fromNodeId.Value))
        {
            throw new InvalidOperationException($"Cannot add an edge from unknown node '{fromNodeId}'.");
        }
        if (!_nodeIds.Contains(toNodeId.Value))
        {
            throw new InvalidOperationException($"Cannot add an edge to unknown node '{toNodeId}'.");
        }

        _edges.Add(new EnterpriseGraphEdge(fromNodeId, toNodeId, edgeType, nowUtc));
        return this;
    }

    /// <summary>Builds the immutable <see cref="EnterpriseGraph"/> from every node/edge added so far.</summary>
    public EnterpriseGraph Build() => new([.. _nodes], [.. _edges]);
}
