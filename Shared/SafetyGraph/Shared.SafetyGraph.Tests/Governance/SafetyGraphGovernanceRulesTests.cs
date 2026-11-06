// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.SafetyGraph.Abstractions.Governance;
using Shared.SafetyGraph.Abstractions.Graph;

using Xunit;

namespace Shared.SafetyGraph.Tests.Governance;

public sealed class SafetyGraphGovernanceRulesTests
{
    private static SafetyGraphNode MakeNode(string id) => new(id, SafetyGraphNodeType.Facility, id, new Dictionary<string, string>());

    [Fact]
    public void ValidateContractMetadata_ValidMetadata_ReturnsNoViolations()
    {
        var metadata = new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);

        SafetyGraphGovernanceRules.ValidateContractMetadata(metadata).Should().BeEmpty();
    }

    [Fact]
    public void ValidateContractMetadata_MissingTenantId_ReturnsViolation()
    {
        var metadata = new SafetyGraphMetadata(SafetyGraphMetadata.CurrentSchemaVersion, string.Empty, "corr-1", DateTime.UtcNow);

        SafetyGraphGovernanceRules.ValidateContractMetadata(metadata).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateGraphStructure_DanglingEdgeReference_ReturnsViolation()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("A") },
            new[] { new SafetyGraphEdge("A->B", "A", "B", SafetyGraphEdgeType.Causal, "") });

        SafetyGraphGovernanceRules.ValidateGraphStructure(graph).Should().Contain(v => v.Contains("TargetNodeId"));
    }

    [Fact]
    public void ValidateGraphStructure_UnsortedNodes_ReturnsViolation()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("B"), MakeNode("A") },
            Array.Empty<SafetyGraphEdge>());

        SafetyGraphGovernanceRules.ValidateGraphStructure(graph).Should().Contain(v => v.Contains("Nodes are not sorted"));
    }

    [Fact]
    public void ValidateGraphStructure_ValidGraph_ReturnsNoViolations()
    {
        var graph = new SafetyGraphModel(
            new SafetyGraphMetadata(1, "acme", "corr-1", DateTime.UtcNow),
            new[] { MakeNode("A"), MakeNode("B") },
            new[] { new SafetyGraphEdge("A->B:Causal", "A", "B", SafetyGraphEdgeType.Causal, "") });

        SafetyGraphGovernanceRules.ValidateGraphStructure(graph).Should().BeEmpty();
    }

    [Fact]
    public void ValidateEscalationOutcome_InconsistentOutcome_ReturnsViolation()
    {
        var outcome = new PredictiveEscalationOutcome(true, Array.Empty<string>());

        SafetyGraphGovernanceRules.ValidateEscalationOutcome(outcome).Should().NotBeEmpty();
    }
}
