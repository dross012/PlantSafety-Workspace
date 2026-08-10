// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Traversal
//  File:           SafetyGraphTraversalRules.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministic, pure adjacency
//                  construction shared by every SafetyGraphTraversalEngine
//                  method. Adjacency lists are built once per call and
//                  sorted by target NodeId (ordinal) -- this single sort is
//                  what makes every traversal below deterministic, since a
//                  SafetyGraph's own Edges list is already sorted by
//                  (SourceNodeId, TargetNodeId, EdgeType, EdgeId) at
//                  construction time (SafetyGraphAssembler), but a filtered
//                  subset (e.g. Dependency-only edges) needs its own
//                  re-verified deterministic order.
// ============================================================================
using Shared.SafetyGraph.Abstractions.Graph;

namespace Shared.SafetyGraph.Abstractions.Traversal;

/// <summary>Deterministic, pure adjacency construction for graph traversal.</summary>
public static class SafetyGraphTraversalRules
{
    /// <summary>Builds a source-NodeId -> sorted-target-NodeId-list adjacency map over every edge in <paramref name="graph"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildAdjacency(SafetyGraphModel graph)
        => BuildAdjacency(graph, edgeTypeFilter: null);

    /// <summary>
    /// Builds a source-NodeId -> sorted-target-NodeId-list adjacency map, restricted to edges whose
    /// EdgeType equals <paramref name="edgeTypeFilter"/> when supplied.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildAdjacency(SafetyGraphModel graph, SafetyGraphEdgeType? edgeTypeFilter)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (SafetyGraphEdge edge in graph.Edges)
        {
            if (edgeTypeFilter is not null && edge.EdgeType != edgeTypeFilter.Value)
            {
                continue;
            }

            if (!adjacency.TryGetValue(edge.SourceNodeId, out List<string>? targets))
            {
                targets = new List<string>();
                adjacency[edge.SourceNodeId] = targets;
            }

            targets.Add(edge.TargetNodeId);
        }

        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach ((string source, List<string> targets) in adjacency)
        {
            targets.Sort(StringComparer.Ordinal);
            result[source] = targets;
        }

        return result;
    }
}
