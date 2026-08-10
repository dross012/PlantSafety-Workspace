// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Abstractions.Governance;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Governance;

public sealed class PredictiveGovernanceRulesTests
{
    [Fact]
    public void ValidateContractMetadata_ValidMetadata_ReturnsNoViolations()
    {
        var metadata = new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);

        PredictiveGovernanceRules.ValidateContractMetadata(metadata).Should().BeEmpty();
    }

    [Fact]
    public void ValidateContractMetadata_MissingTenantId_ReturnsViolation()
    {
        var metadata = new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, string.Empty, "corr-1", DateTime.UtcNow);

        PredictiveGovernanceRules.ValidateContractMetadata(metadata).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateEscalationOutcome_EscalationTrueWithNoCodes_ReturnsViolation()
    {
        var outcome = new PredictiveEscalationOutcome(true, Array.Empty<string>());

        PredictiveGovernanceRules.ValidateEscalationOutcome(outcome).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateEscalationOutcome_EscalationFalseWithCodes_ReturnsViolation()
    {
        var outcome = new PredictiveEscalationOutcome(false, new[] { "UNEXPECTED-RULE" });

        PredictiveGovernanceRules.ValidateEscalationOutcome(outcome).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateEscalationOutcome_ConsistentOutcome_ReturnsNoViolations()
    {
        var outcome = new PredictiveEscalationOutcome(true, new[] { "RULE-1" });

        PredictiveGovernanceRules.ValidateEscalationOutcome(outcome).Should().BeEmpty();
    }
}
