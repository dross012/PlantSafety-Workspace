// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyGraph.Tests.Governance;

public sealed class SafetyGraphGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly DateTime OccurredAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void EvaluateContractMetadataAndReport_ValidMetadata_ReturnsEmptyAndDoesNotWrite()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", OccurredAtUtc);

        SafetyGraphGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateContractMetadataAndReport_InvalidMetadata_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var metadata = new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, string.Empty, "corr-2", OccurredAtUtc);

        SafetyGraphGovernanceEvaluator.EvaluateContractMetadataAndReport(metadata, OccurredAtUtc, writer).Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("SAFETY_GRAPH_GOVERNANCE_VIOLATION");
    }

    [Fact]
    public void EvaluateGraphStructureAndReport_DanglingEdge_WritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var node = new SafetyGraphNode("A", SafetyGraphNodeType.Facility, "A", new Dictionary<string, string>());
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", OccurredAtUtc),
            new[] { node },
            new[] { new SafetyGraphEdge("A->B", "A", "B", SafetyGraphEdgeType.Causal, "") });

        SafetyGraphGovernanceEvaluator.EvaluateGraphStructureAndReport(graph, "corr-3", OccurredAtUtc, writer).Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("SAFETY_GRAPH_GOVERNANCE_VIOLATION");
    }
}
