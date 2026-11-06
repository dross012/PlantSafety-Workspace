// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphTraversalEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Traversal;

using Xunit;

namespace Shared.SafetyGraph.Tests.Traversal;

public sealed class SafetyGraphTraversalEngineTests
{
    private static SafetyGraphNode MakeNode(string id) => new(id, SafetyGraphNodeType.Facility, id, new Dictionary<string, string>());

    private static SafetyGraphEdge MakeEdge(string source, string target, SafetyGraphEdgeType edgeType = SafetyGraphEdgeType.Causal)
        => new($"{source}->{target}:{edgeType}", source, target, edgeType, "");

    // A -> B -> D
    // A -> C -> D
    private static SafetyGraphModel DiamondGraph() => new(
        new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
        new[] { MakeNode("A"), MakeNode("B"), MakeNode("C"), MakeNode("D") },
        new[] { MakeEdge("A", "B"), MakeEdge("A", "C"), MakeEdge("B", "D"), MakeEdge("C", "D") });

    [Fact]
    public void TraverseDepthFirst_VisitsInDeterministicOrder()
    {
        SafetyGraphModel graph = DiamondGraph();

        SafetyGraphTraversalResult result = SafetyGraphTraversalEngine.TraverseDepthFirst(graph, "A");

        result.VisitedNodeIds.Should().Equal("A", "B", "D", "C");
    }

    [Fact]
    public void TraverseDepthFirst_SameGraphTwice_ProducesIdenticalResult()
    {
        SafetyGraphModel graph = DiamondGraph();

        SafetyGraphTraversalEngine.TraverseDepthFirst(graph, "A")
            .Should().BeEquivalentTo(SafetyGraphTraversalEngine.TraverseDepthFirst(graph, "A"));
    }

    [Fact]
    public void TraverseBreadthFirst_VisitsInDeterministicOrder()
    {
        SafetyGraphModel graph = DiamondGraph();

        SafetyGraphTraversalResult result = SafetyGraphTraversalEngine.TraverseBreadthFirst(graph, "A");

        result.VisitedNodeIds.Should().Equal("A", "B", "C", "D");
    }

    [Fact]
    public void EnumeratePaths_FindsBothDiamondPaths()
    {
        SafetyGraphModel graph = DiamondGraph();

        SafetyGraphTraversalResult result = SafetyGraphTraversalEngine.EnumeratePaths(graph, "A", "D");

        result.EnumeratedPaths.Should().HaveCount(2);
        result.EnumeratedPaths.Should().ContainEquivalentOf(new[] { "A", "B", "D" });
        result.EnumeratedPaths.Should().ContainEquivalentOf(new[] { "A", "C", "D" });
    }

    [Fact]
    public void EnumeratePaths_NoPathExists_ReturnsEmpty()
    {
        SafetyGraphModel graph = DiamondGraph();

        SafetyGraphTraversalEngine.EnumeratePaths(graph, "D", "A").EnumeratedPaths.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycle_AcyclicGraph_ReturnsFalse()
    {
        SafetyGraphModel graph = DiamondGraph();

        SafetyGraphTraversalEngine.DetectCycle(graph).CycleDetected.Should().BeFalse();
    }

    [Fact]
    public void DetectCycle_CyclicGraph_ReturnsTrueWithCycleNodes()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("A"), MakeNode("B"), MakeNode("C") },
            new[] { MakeEdge("A", "B"), MakeEdge("B", "C"), MakeEdge("C", "A") });

        SafetyGraphTraversalResult result = SafetyGraphTraversalEngine.DetectCycle(graph);

        result.CycleDetected.Should().BeTrue();
        result.CycleNodeIds.Should().Contain("A").And.Contain("B").And.Contain("C");
    }

    [Fact]
    public void ComputeReachableNodeIds_MatchesDepthFirstVisitedSet()
    {
        SafetyGraphModel graph = DiamondGraph();

        SafetyGraphTraversalEngine.ComputeReachableNodeIds(graph, "A")
            .Should().Equal(SafetyGraphTraversalEngine.TraverseDepthFirst(graph, "A").VisitedNodeIds);
    }

    [Fact]
    public void ResolveDependencies_OnlyFollowsDependencyEdges()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("A"), MakeNode("B"), MakeNode("C") },
            new[] { MakeEdge("A", "B", SafetyGraphEdgeType.Dependency), MakeEdge("A", "C", SafetyGraphEdgeType.Causal) });

        IReadOnlyList<string> dependencies = SafetyGraphTraversalEngine.ResolveDependencies(graph, "A");

        dependencies.Should().ContainSingle().Which.Should().Be("B");
    }

    [Fact]
    public void TraverseDepthFirst_NullGraph_Throws()
    {
        Action act = () => SafetyGraphTraversalEngine.TraverseDepthFirst(null!, "A");

        act.Should().Throw<ArgumentNullException>();
    }
}
