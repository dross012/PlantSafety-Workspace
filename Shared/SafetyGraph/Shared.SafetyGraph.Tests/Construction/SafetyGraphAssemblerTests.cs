// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphAssemblerTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.SafetyGraph.Abstractions.Construction;
using Shared.SafetyGraph.Abstractions.Graph;

using Xunit;

namespace Shared.SafetyGraph.Tests.Construction;

public sealed class SafetyGraphAssemblerTests
{
    private static SafetyGraphConstructionRequestContract BuildRequest() => new(
        new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow),
        FacilityId: Guid.NewGuid(),
        FacilityName: "Plant One",
        EmployeeId: null,
        MachineId: Guid.NewGuid(),
        WorkflowId: null,
        RoutingResult: null,
        SchedulingDecision: null,
        PredictiveResult: null);

    [Fact]
    public void Assemble_MinimalRequest_ProducesFacilityAndMachineNodes()
    {
        SafetyGraphModel graph = SafetyGraphAssembler.Assemble(BuildRequest());

        graph.Nodes.Should().Contain(n => n.NodeType == SafetyGraphNodeType.Facility);
        graph.Nodes.Should().Contain(n => n.NodeType == SafetyGraphNodeType.Machine);
        graph.Edges.Should().ContainSingle(e => e.EdgeType == SafetyGraphEdgeType.Spatial);
    }

    [Fact]
    public void Assemble_WithEmployeeId_AddsEmployeeNodeAndOrganizationalEdge()
    {
        SafetyGraphConstructionRequestContract request = BuildRequest() with { EmployeeId = Guid.NewGuid() };

        SafetyGraphModel graph = SafetyGraphAssembler.Assemble(request);

        graph.Nodes.Should().Contain(n => n.NodeType == SafetyGraphNodeType.Employee);
        graph.Edges.Should().Contain(e => e.EdgeType == SafetyGraphEdgeType.Organizational);
    }

    [Fact]
    public void Assemble_NodesAreSortedByNodeIdOrdinal()
    {
        SafetyGraphConstructionRequestContract request = BuildRequest() with { EmployeeId = Guid.NewGuid(), WorkflowId = "wf-1" };

        SafetyGraphModel graph = SafetyGraphAssembler.Assemble(request);

        graph.Nodes.Select(n => n.NodeId).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Fact]
    public void Assemble_EdgesAreSortedBySourceThenTarget()
    {
        SafetyGraphConstructionRequestContract request = BuildRequest() with { EmployeeId = Guid.NewGuid() };

        SafetyGraphModel graph = SafetyGraphAssembler.Assemble(request);

        graph.Edges.Select(e => e.SourceNodeId).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Fact]
    public void Assemble_SameInputTwice_ProducesIdenticalGraph()
    {
        SafetyGraphConstructionRequestContract request = BuildRequest();

        SafetyGraphAssembler.Assemble(request).Should().BeEquivalentTo(SafetyGraphAssembler.Assemble(request));
    }

    [Fact]
    public void Assemble_NullRequest_Throws()
    {
        Action act = () => SafetyGraphAssembler.Assemble(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
