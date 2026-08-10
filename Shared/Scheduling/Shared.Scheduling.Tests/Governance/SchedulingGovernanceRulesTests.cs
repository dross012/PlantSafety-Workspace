// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Governance;
using Shared.Scheduling.Abstractions.Rules;

using Xunit;

namespace Shared.Scheduling.Tests.Governance;

public sealed class SchedulingGovernanceRulesTests
{
    [Fact]
    public void ValidateContractMetadata_ValidMetadata_ReturnsNoViolations()
    {
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);

        SchedulingGovernanceRules.ValidateContractMetadata(metadata).Should().BeEmpty();
    }

    [Fact]
    public void ValidateContractMetadata_MissingTenantId_ReturnsViolation()
    {
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, string.Empty, "corr-1", DateTime.UtcNow);

        SchedulingGovernanceRules.ValidateContractMetadata(metadata).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateContractMetadata_WrongSchemaVersion_ReturnsViolation()
    {
        var metadata = new SchedulingContractMetadata(99, "acme-plant-1", "corr-1", DateTime.UtcNow);

        SchedulingGovernanceRules.ValidateContractMetadata(metadata).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateContractMetadata_DefaultIssuedAtUtc_ReturnsViolation()
    {
        var metadata = new SchedulingContractMetadata(SchedulingContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", default);

        SchedulingGovernanceRules.ValidateContractMetadata(metadata).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateDueTime_MissingActionCode_ReturnsViolation()
    {
        var dueTime = new ScheduledDueTime(string.Empty, "Description", DateTime.UtcNow, IsOverdue: false);

        SchedulingGovernanceRules.ValidateDueTime(dueTime).Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateDueTime_ValidDueTime_ReturnsNoViolations()
    {
        var dueTime = new ScheduledDueTime("ACT-1", "Description", DateTime.UtcNow, IsOverdue: false);

        SchedulingGovernanceRules.ValidateDueTime(dueTime).Should().BeEmpty();
    }

    [Fact]
    public void ValidateRuleDecision_NegativeCounts_ReturnsViolations()
    {
        var decision = new SchedulingRuleDecision(-1, -1, int.MaxValue, false, Array.Empty<string>());

        SchedulingGovernanceRules.ValidateRuleDecision(decision).Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void ValidateRuleDecision_RuleCodesWithZeroCount_ReturnsViolation()
    {
        var decision = new SchedulingRuleDecision(0, 0, int.MaxValue, false, new[] { "UNEXPECTED-RULE" });

        SchedulingGovernanceRules.ValidateRuleDecision(decision).Should().NotBeEmpty();
    }
}
