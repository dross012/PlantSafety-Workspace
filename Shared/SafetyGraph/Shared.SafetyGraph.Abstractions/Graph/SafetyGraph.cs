// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Graph
//  File:           SafetyGraph.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The immutable, serializable,
//                  versioned graph aggregate: a metadata envelope plus a
//                  deterministically-ordered node list and edge list.
//                  SafetyGraphAssembler is the only place a SafetyGraphModel
//                  is constructed with unsorted input -- by the time an
//                  instance exists, Nodes/Edges are already sorted (NodeId
//                  ordinal; (SourceNodeId, TargetNodeId, EdgeType, EdgeId)
//                  ordinal), so every query method below is itself
//                  deterministic without re-sorting.
//
//                  Named SafetyGraphModel, not SafetyGraph: this project's
//                  own root namespace is Shared.SafetyGraph, and C#'s simple-
//                  name lookup resolves an unqualified "SafetyGraph" to that
//                  enclosing namespace before ever considering a type of the
//                  same name (CS0118, confirmed while building this file) --
//                  a type cannot share a bare name with an ancestor
//                  namespace segment without every consumer qualifying it.
//                  The file is still named SafetyGraph.cs, matching this
//                  phase's own required output structure.
// ============================================================================
namespace Shared.SafetyGraph.Abstractions.Graph;

/// <summary>The immutable, deterministically-ordered Safety Graph aggregate.</summary>
public sealed record SafetyGraphModel(
    SafetyGraphMetadata Metadata,
    IReadOnlyList<SafetyGraphNode> Nodes,
    IReadOnlyList<SafetyGraphEdge> Edges)
{
    /// <summary>Finds the node with the given <paramref name="nodeId"/>, or null if none exists.</summary>
    public SafetyGraphNode? FindNode(string nodeId)
    {
        foreach (SafetyGraphNode node in Nodes)
        {
            if (string.Equals(node.NodeId, nodeId, StringComparison.Ordinal))
            {
                return node;
            }
        }

        return null;
    }

    /// <summary>Returns every edge whose SourceNodeId equals <paramref name="nodeId"/>, in the graph's own deterministic edge order.</summary>
    public IReadOnlyList<SafetyGraphEdge> EdgesFrom(string nodeId)
    {
        var results = new List<SafetyGraphEdge>();
        foreach (SafetyGraphEdge edge in Edges)
        {
            if (string.Equals(edge.SourceNodeId, nodeId, StringComparison.Ordinal))
            {
                results.Add(edge);
            }
        }

        return results;
    }

    /// <summary>Returns every edge whose TargetNodeId equals <paramref name="nodeId"/>, in the graph's own deterministic edge order.</summary>
    public IReadOnlyList<SafetyGraphEdge> EdgesTo(string nodeId)
    {
        var results = new List<SafetyGraphEdge>();
        foreach (SafetyGraphEdge edge in Edges)
        {
            if (string.Equals(edge.TargetNodeId, nodeId, StringComparison.Ordinal))
            {
                results.Add(edge);
            }
        }

        return results;
    }
}
