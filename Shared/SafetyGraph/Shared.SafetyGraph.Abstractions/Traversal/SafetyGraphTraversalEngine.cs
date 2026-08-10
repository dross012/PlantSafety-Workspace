// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Traversal
//  File:           SafetyGraphTraversalEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Deterministic, pure graph traversal:
//                  depth-first, breadth-first, simple-path enumeration,
//                  cycle detection (classic white/gray/black DFS), full-graph
//                  reachability, and Dependency-edge-only dependency
//                  resolution. Every method walks SafetyGraphTraversalRules.
//                  BuildAdjacency's sorted adjacency lists, so the same
//                  graph and start node always produce the same visitation
//                  order, the same enumerated paths, and the same cycle (if
//                  any) -- no heuristic, no probabilistic, no ML traversal
//                  anywhere in this file.
// ============================================================================
using Shared.SafetyGraph.Abstractions.Graph;

namespace Shared.SafetyGraph.Abstractions.Traversal;

/// <summary>Deterministic, pure graph traversal algorithms.</summary>
public static class SafetyGraphTraversalEngine
{
    private static readonly IReadOnlyList<IReadOnlyList<string>> NoPaths = Array.Empty<IReadOnlyList<string>>();
    private static readonly IReadOnlyList<string> NoCycle = Array.Empty<string>();

    /// <summary>Deterministically visits every node reachable from <paramref name="startNodeId"/> in depth-first order.</summary>
    public static SafetyGraphTraversalResult TraverseDepthFirst(SafetyGraphModel graph, string startNodeId)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(startNodeId);

        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency = SafetyGraphTraversalRules.BuildAdjacency(graph);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var order = new List<string>();
        var stack = new Stack<string>();
        stack.Push(startNodeId);

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
                // Push in reverse-sorted order so the smallest target is popped (visited) first,
                // preserving deterministic ascending visitation order for equal-priority branches.
                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    if (!visited.Contains(targets[i]))
                    {
                        stack.Push(targets[i]);
                    }
                }
            }
        }

        return new SafetyGraphTraversalResult(order, NoPaths, CycleDetected: false, NoCycle);
    }

    /// <summary>Deterministically visits every node reachable from <paramref name="startNodeId"/> in breadth-first order.</summary>
    public static SafetyGraphTraversalResult TraverseBreadthFirst(SafetyGraphModel graph, string startNodeId)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(startNodeId);

        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency = SafetyGraphTraversalRules.BuildAdjacency(graph);
        var visited = new HashSet<string>(StringComparer.Ordinal) { startNodeId };
        var order = new List<string> { startNodeId };
        var queue = new Queue<string>();
        queue.Enqueue(startNodeId);

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            if (!adjacency.TryGetValue(current, out IReadOnlyList<string>? targets))
            {
                continue;
            }

            foreach (string target in targets)
            {
                if (visited.Add(target))
                {
                    order.Add(target);
                    queue.Enqueue(target);
                }
            }
        }

        return new SafetyGraphTraversalResult(order, NoPaths, CycleDetected: false, NoCycle);
    }

    /// <summary>Deterministically enumerates every simple (non-repeating) path from <paramref name="startNodeId"/> to <paramref name="endNodeId"/>.</summary>
    public static SafetyGraphTraversalResult EnumeratePaths(SafetyGraphModel graph, string startNodeId, string endNodeId)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(startNodeId);
        ArgumentNullException.ThrowIfNull(endNodeId);

        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency = SafetyGraphTraversalRules.BuildAdjacency(graph);
        var paths = new List<IReadOnlyList<string>>();
        var currentPath = new List<string> { startNodeId };
        var onPath = new HashSet<string>(StringComparer.Ordinal) { startNodeId };

        EnumeratePathsRecursive(startNodeId, endNodeId, adjacency, currentPath, onPath, paths);

        return new SafetyGraphTraversalResult(Array.Empty<string>(), paths, CycleDetected: false, NoCycle);
    }

    private static void EnumeratePathsRecursive(
        string current,
        string endNodeId,
        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency,
        List<string> currentPath,
        HashSet<string> onPath,
        List<IReadOnlyList<string>> paths)
    {
        if (string.Equals(current, endNodeId, StringComparison.Ordinal))
        {
            paths.Add(currentPath.ToArray());
            return;
        }

        if (!adjacency.TryGetValue(current, out IReadOnlyList<string>? targets))
        {
            return;
        }

        foreach (string target in targets)
        {
            if (onPath.Contains(target))
            {
                continue;
            }

            currentPath.Add(target);
            onPath.Add(target);

            EnumeratePathsRecursive(target, endNodeId, adjacency, currentPath, onPath, paths);

            onPath.Remove(target);
            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }

    /// <summary>Deterministically detects a cycle anywhere in <paramref name="graph"/> via a classic white/gray/black DFS over every node, in NodeId order.</summary>
    public static SafetyGraphTraversalResult DetectCycle(SafetyGraphModel graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency = SafetyGraphTraversalRules.BuildAdjacency(graph);
        var visitedState = new Dictionary<string, int>(StringComparer.Ordinal); // 0=white, 1=gray, 2=black
        var pathStack = new List<string>();

        foreach (SafetyGraphNode node in graph.Nodes)
        {
            if (visitedState.GetValueOrDefault(node.NodeId, 0) != 0)
            {
                continue;
            }

            IReadOnlyList<string>? cycle = DetectCycleFrom(node.NodeId, adjacency, visitedState, pathStack);
            if (cycle is not null)
            {
                return new SafetyGraphTraversalResult(Array.Empty<string>(), NoPaths, CycleDetected: true, cycle);
            }
        }

        return new SafetyGraphTraversalResult(Array.Empty<string>(), NoPaths, CycleDetected: false, NoCycle);
    }

    private static IReadOnlyList<string>? DetectCycleFrom(
        string startNodeId,
        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency,
        Dictionary<string, int> visitedState,
        List<string> pathStack)
    {
        visitedState[startNodeId] = 1;
        pathStack.Add(startNodeId);

        if (adjacency.TryGetValue(startNodeId, out IReadOnlyList<string>? targets))
        {
            foreach (string target in targets)
            {
                int state = visitedState.GetValueOrDefault(target, 0);
                if (state == 1)
                {
                    int cycleStartIndex = pathStack.IndexOf(target);
                    var cycle = pathStack.GetRange(cycleStartIndex, pathStack.Count - cycleStartIndex);
                    cycle.Add(target);
                    return cycle;
                }

                if (state == 0)
                {
                    IReadOnlyList<string>? found = DetectCycleFrom(target, adjacency, visitedState, pathStack);
                    if (found is not null)
                    {
                        return found;
                    }
                }
            }
        }

        pathStack.RemoveAt(pathStack.Count - 1);
        visitedState[startNodeId] = 2;
        return null;
    }

    /// <summary>Deterministically computes the set of node ids reachable from <paramref name="startNodeId"/> (equivalent to TraverseDepthFirst's visited set).</summary>
    public static IReadOnlyList<string> ComputeReachableNodeIds(SafetyGraphModel graph, string startNodeId)
        => TraverseDepthFirst(graph, startNodeId).VisitedNodeIds;

    /// <summary>Deterministically resolves every node <paramref name="nodeId"/> transitively depends on, following only Dependency-typed edges.</summary>
    public static IReadOnlyList<string> ResolveDependencies(SafetyGraphModel graph, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(nodeId);

        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency = SafetyGraphTraversalRules.BuildAdjacency(graph, SafetyGraphEdgeType.Dependency);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var order = new List<string>();
        var stack = new Stack<string>();
        stack.Push(nodeId);

        while (stack.Count > 0)
        {
            string current = stack.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            if (!string.Equals(current, nodeId, StringComparison.Ordinal))
            {
                order.Add(current);
            }

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

        return order.OrderBy(id => id, StringComparer.Ordinal).ToArray();
    }
}
