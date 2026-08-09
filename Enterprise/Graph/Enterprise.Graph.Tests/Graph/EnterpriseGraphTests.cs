// ============================================================================
//  Project:        Enterprise.Graph.Tests
//  File:           EnterpriseGraphTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.1 unit tests for EnterpriseGraphBuilder/
//                  EnterpriseGraph: node/edge construction, dangling-edge
//                  and duplicate-node guards, and query helpers
//                  (NodesOfType/EdgesFrom/EdgesTo/Neighbors).
// ============================================================================
using Enterprise.Graph.Abstractions.Edges;
using Enterprise.Graph.Abstractions.Enums;
using Enterprise.Graph.Abstractions.Graph;
using Enterprise.Graph.Abstractions.Ids;
using Enterprise.Graph.Abstractions.Nodes;

using FluentAssertions;

using Xunit;

namespace Enterprise.Graph.Tests.Graph;

public sealed class EnterpriseGraphTests
{
    [Fact]
    public void Build_AddsNodesAndEdgesInOrder()
    {
        var facilityNodeId = EnterpriseNodeId.New();
        var employeeNodeId = EnterpriseNodeId.New();
        var now = DateTime.UtcNow;

        EnterpriseGraph graph = new EnterpriseGraphBuilder()
            .AddNode(facilityNodeId, EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 1", now)
            .AddNode(employeeNodeId, EnterpriseNodeType.Employee, Guid.NewGuid(), "Jane Doe", now)
            .AddEdge(employeeNodeId, facilityNodeId, EnterpriseEdgeType.LocatedIn, now)
            .Build();

        graph.Nodes.Should().HaveCount(2);
        graph.Edges.Should().ContainSingle();
        graph.Edges[0].EdgeType.Should().Be(EnterpriseEdgeType.LocatedIn);
    }

    [Fact]
    public void AddNode_DuplicateId_Throws()
    {
        var nodeId = EnterpriseNodeId.New();
        var builder = new EnterpriseGraphBuilder().AddNode(nodeId, EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 1", DateTime.UtcNow);

        Action act = () => builder.AddNode(nodeId, EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 2", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddNode_BlankLabel_Throws()
    {
        var builder = new EnterpriseGraphBuilder();

        Action act = () => builder.AddNode(EnterpriseNodeId.New(), EnterpriseNodeType.Facility, Guid.NewGuid(), " ", DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddEdge_UnknownFromNode_Throws()
    {
        var builder = new EnterpriseGraphBuilder()
            .AddNode(EnterpriseNodeId.New(), EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 1", DateTime.UtcNow);

        Action act = () => builder.AddEdge(EnterpriseNodeId.New(), EnterpriseNodeId.New(), EnterpriseEdgeType.LocatedIn, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddEdge_UnknownToNode_Throws()
    {
        var fromNodeId = EnterpriseNodeId.New();
        var builder = new EnterpriseGraphBuilder()
            .AddNode(fromNodeId, EnterpriseNodeType.Employee, Guid.NewGuid(), "Jane Doe", DateTime.UtcNow);

        Action act = () => builder.AddEdge(fromNodeId, EnterpriseNodeId.New(), EnterpriseEdgeType.LocatedIn, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NodesOfType_ReturnsOnlyMatchingNodes()
    {
        var now = DateTime.UtcNow;
        EnterpriseGraph graph = new EnterpriseGraphBuilder()
            .AddNode(EnterpriseNodeId.New(), EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 1", now)
            .AddNode(EnterpriseNodeId.New(), EnterpriseNodeType.Employee, Guid.NewGuid(), "Jane Doe", now)
            .AddNode(EnterpriseNodeId.New(), EnterpriseNodeType.Employee, Guid.NewGuid(), "John Roe", now)
            .Build();

        graph.NodesOfType(EnterpriseNodeType.Employee).Should().HaveCount(2);
        graph.NodesOfType(EnterpriseNodeType.Machine).Should().BeEmpty();
    }

    [Fact]
    public void EdgesFromAndEdgesTo_ReturnCorrectDirection()
    {
        var facilityNodeId = EnterpriseNodeId.New();
        var employeeNodeId = EnterpriseNodeId.New();
        var now = DateTime.UtcNow;
        EnterpriseGraph graph = new EnterpriseGraphBuilder()
            .AddNode(facilityNodeId, EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 1", now)
            .AddNode(employeeNodeId, EnterpriseNodeType.Employee, Guid.NewGuid(), "Jane Doe", now)
            .AddEdge(employeeNodeId, facilityNodeId, EnterpriseEdgeType.LocatedIn, now)
            .Build();

        graph.EdgesFrom(employeeNodeId).Should().ContainSingle();
        graph.EdgesTo(facilityNodeId).Should().ContainSingle();
        graph.EdgesFrom(facilityNodeId).Should().BeEmpty();
    }

    [Fact]
    public void Neighbors_ReturnsDirectlyConnectedNodes_FilteredByEdgeTypeWhenSpecified()
    {
        var employeeNodeId = EnterpriseNodeId.New();
        var facilityNodeId = EnterpriseNodeId.New();
        var machineNodeId = EnterpriseNodeId.New();
        var now = DateTime.UtcNow;
        EnterpriseGraph graph = new EnterpriseGraphBuilder()
            .AddNode(employeeNodeId, EnterpriseNodeType.Employee, Guid.NewGuid(), "Jane Doe", now)
            .AddNode(facilityNodeId, EnterpriseNodeType.Facility, Guid.NewGuid(), "Plant 1", now)
            .AddNode(machineNodeId, EnterpriseNodeType.Machine, Guid.NewGuid(), "Press 1", now)
            .AddEdge(employeeNodeId, facilityNodeId, EnterpriseEdgeType.LocatedIn, now)
            .AddEdge(employeeNodeId, machineNodeId, EnterpriseEdgeType.Operates, now)
            .Build();

        graph.Neighbors(employeeNodeId).Should().HaveCount(2);
        graph.Neighbors(employeeNodeId, EnterpriseEdgeType.Operates).Should().ContainSingle(n => n.NodeId == machineNodeId);
    }
}
