// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphInferenceEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Abstractions.Inference;

using Xunit;

namespace Shared.SafetyGraph.Tests.Inference;

public sealed class SafetyGraphInferenceEngineTests
{
    private static SafetyGraphNode MakeNode(string id, SafetyGraphNodeType type) => new(id, type, id, new Dictionary<string, string>());

    private static SafetyGraphEdge MakeEdge(string source, string target, SafetyGraphEdgeType edgeType)
        => new($"{source}->{target}:{edgeType}", source, target, edgeType, "");

    [Fact]
    public void Infer_NoPropagationNoPredictive_ReturnsLowPriorityNoEscalation()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("Facility:1", SafetyGraphNodeType.Facility) },
            Array.Empty<SafetyGraphEdge>());

        SafetyGraphInferenceResult result = SafetyGraphInferenceEngine.Infer(graph, predictiveResult: null);

        result.RequiresGraphWideEscalation.Should().BeFalse();
        result.OverallPriority.Should().Be(PredictivePriority.Low);
    }

    [Fact]
    public void Infer_HazardPropagatesViaCausalEdge_Escalates()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("Hazard:1", SafetyGraphNodeType.Hazard), MakeNode("RoutingDecision:1", SafetyGraphNodeType.RoutingDecision) },
            new[] { MakeEdge("Hazard:1", "RoutingDecision:1", SafetyGraphEdgeType.Causal) });

        SafetyGraphInferenceResult result = SafetyGraphInferenceEngine.Infer(graph, predictiveResult: null);

        result.RequiresGraphWideEscalation.Should().BeTrue();
        result.AppliedRuleCodes.Should().Contain("HAZARD-PROPAGATION-ESCALATION");
        result.HazardPropagationNodeIds.Should().Contain("RoutingDecision:1");
    }

    [Fact]
    public void Infer_MaintenancePropagatesViaDependencyEdge_Escalates()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("Machine:1", SafetyGraphNodeType.Machine), MakeNode("SchedulingDecision:1", SafetyGraphNodeType.SchedulingDecision) },
            new[] { MakeEdge("Machine:1", "SchedulingDecision:1", SafetyGraphEdgeType.Dependency) });

        SafetyGraphInferenceResult result = SafetyGraphInferenceEngine.Infer(graph, predictiveResult: null);

        result.AppliedRuleCodes.Should().Contain("MAINTENANCE-PROPAGATION-ESCALATION");
        result.MaintenancePropagationNodeIds.Should().Contain("SchedulingDecision:1");
    }

    [Fact]
    public void Infer_HazardCycle_SetsCycleDetectedOnPropagationResult()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("Hazard:1", SafetyGraphNodeType.Hazard), MakeNode("RoutingDecision:1", SafetyGraphNodeType.RoutingDecision) },
            new[]
            {
                MakeEdge("Hazard:1", "RoutingDecision:1", SafetyGraphEdgeType.Causal),
                MakeEdge("RoutingDecision:1", "Hazard:1", SafetyGraphEdgeType.Causal),
            });

        SafetyGraphInferenceResult result = SafetyGraphInferenceEngine.Infer(graph, predictiveResult: null);

        result.AppliedRuleCodes.Should().Contain("HAZARD-CYCLE-ESCALATION");
    }

    [Fact]
    public void Infer_SameInputTwice_ProducesIdenticalResult()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("Hazard:1", SafetyGraphNodeType.Hazard), MakeNode("RoutingDecision:1", SafetyGraphNodeType.RoutingDecision) },
            new[] { MakeEdge("Hazard:1", "RoutingDecision:1", SafetyGraphEdgeType.Causal) });

        SafetyGraphInferenceEngine.Infer(graph, null).Should().BeEquivalentTo(SafetyGraphInferenceEngine.Infer(graph, null));
    }

    [Fact]
    public void Infer_NullGraph_Throws()
    {
        Action act = () => SafetyGraphInferenceEngine.Infer(null!, null);

        act.Should().Throw<ArgumentNullException>();
    }
}
