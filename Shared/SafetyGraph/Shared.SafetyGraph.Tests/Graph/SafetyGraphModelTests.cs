// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphModelTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.SafetyGraph.Abstractions.Graph;

using Xunit;

namespace Shared.SafetyGraph.Tests.Graph;

public sealed class SafetyGraphModelTests
{
    private static SafetyGraphNode MakeNode(string id) => new(id, SafetyGraphNodeType.Facility, id, new Dictionary<string, string>());

    [Fact]
    public void FindNode_ExistingId_ReturnsNode()
    {
        var node = MakeNode("A");
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { node },
            Array.Empty<SafetyGraphEdge>());

        graph.FindNode("A").Should().Be(node);
    }

    [Fact]
    public void FindNode_MissingId_ReturnsNull()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            Array.Empty<SafetyGraphNode>(),
            Array.Empty<SafetyGraphEdge>());

        graph.FindNode("missing").Should().BeNull();
    }

    [Fact]
    public void EdgesFrom_ReturnsOnlyMatchingSourceEdges()
    {
        var edgeAB = new SafetyGraphEdge("A->B", "A", "B", SafetyGraphEdgeType.Causal, "");
        var edgeBC = new SafetyGraphEdge("B->C", "B", "C", SafetyGraphEdgeType.Causal, "");
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("A"), MakeNode("B"), MakeNode("C") },
            new[] { edgeAB, edgeBC });

        graph.EdgesFrom("A").Should().ContainSingle().Which.Should().Be(edgeAB);
    }

    [Fact]
    public void EdgesTo_ReturnsOnlyMatchingTargetEdges()
    {
        var edgeAB = new SafetyGraphEdge("A->B", "A", "B", SafetyGraphEdgeType.Causal, "");
        var edgeCB = new SafetyGraphEdge("C->B", "C", "B", SafetyGraphEdgeType.Causal, "");
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("A"), MakeNode("B"), MakeNode("C") },
            new[] { edgeAB, edgeCB });

        graph.EdgesTo("B").Should().HaveCount(2);
    }
}
