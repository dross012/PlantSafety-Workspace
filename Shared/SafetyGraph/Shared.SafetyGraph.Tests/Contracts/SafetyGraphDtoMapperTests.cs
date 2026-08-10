// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphDtoMapperTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.SafetyGraph.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Inference;

using Xunit;

namespace Shared.SafetyGraph.Tests.Contracts;

public sealed class SafetyGraphDtoMapperTests
{
    [Fact]
    public void ToDto_ProjectsNodesEdgesAndInference()
    {
        var node = new SafetyGraphNode("Facility:1", SafetyGraphNodeType.Facility, "Plant One", new Dictionary<string, string>());
        var edge = new SafetyGraphEdge("A->B", "Facility:1", "Facility:1", SafetyGraphEdgeType.Spatial, "self");
        var graph = new SafetyGraphModel(new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow), new[] { node }, new[] { edge });
        var inference = new SafetyGraphInferenceResult(true, PredictivePriority.High, new[] { "RULE-1" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
        var result = new SafetyGraphEvaluationResultContract(graph.Metadata, graph, inference);

        SafetyGraphEvaluationDto dto = SafetyGraphDtoMapper.ToDto(result);

        dto.Nodes.Should().ContainSingle().Which.NodeId.Should().Be("Facility:1");
        dto.Edges.Should().ContainSingle();
        dto.RequiresGraphWideEscalation.Should().BeTrue();
        dto.OverallPriority.Should().Be("High");
    }

    [Fact]
    public void ToDto_NullResult_Throws()
    {
        Action act = () => SafetyGraphDtoMapper.ToDto(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
