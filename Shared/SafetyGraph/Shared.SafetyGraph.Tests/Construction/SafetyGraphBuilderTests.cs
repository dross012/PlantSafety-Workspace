// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphBuilderTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.SafetyGraph.Abstractions.Construction;
using Shared.SafetyGraph.Abstractions.Graph;

using Xunit;

namespace Shared.SafetyGraph.Tests.Construction;

public sealed class SafetyGraphBuilderTests
{
    [Fact]
    public void BuildFacilityNode_ProducesDeterministicNodeId()
    {
        Guid facilityId = Guid.NewGuid();

        SafetyGraphNode node = SafetyGraphBuilder.BuildFacilityNode(facilityId, "Plant One");

        node.NodeId.Should().Be($"Facility:{facilityId}");
        node.NodeType.Should().Be(SafetyGraphNodeType.Facility);
        node.Label.Should().Be("Plant One");
    }

    [Fact]
    public void BuildFacilityNode_SameInputTwice_ProducesIdenticalNodeId()
    {
        Guid facilityId = Guid.NewGuid();

        SafetyGraphBuilder.BuildFacilityNode(facilityId, "Plant One").NodeId
            .Should().Be(SafetyGraphBuilder.BuildFacilityNode(facilityId, "Plant One").NodeId);
    }

    [Fact]
    public void BuildEdge_ProducesDeterministicEdgeId()
    {
        SafetyGraphEdge edge = SafetyGraphBuilder.BuildEdge("A", "B", SafetyGraphEdgeType.Causal, "test");

        edge.EdgeId.Should().Be("A->B:Causal");
        edge.SourceNodeId.Should().Be("A");
        edge.TargetNodeId.Should().Be("B");
    }

    [Fact]
    public void BuildMachineNode_NullMachineId_DoesNotThrow()
    {
        // Guid.Empty is a valid, deterministic value -- not a null-argument case.
        SafetyGraphNode node = SafetyGraphBuilder.BuildMachineNode(Guid.Empty);

        node.NodeId.Should().Be($"Machine:{Guid.Empty}");
    }

    [Fact]
    public void BuildWorkflowNode_NullWorkflowId_Throws()
    {
        Action act = () => SafetyGraphBuilder.BuildWorkflowNode(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
