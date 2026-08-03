// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.GlobalSafetyInsights.Coordination
//  Layer:          Tests (Unit)
//  File:           MultiSiteSafetyPolicyTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for MultiSiteSafetyPolicy: site eligibility (invalid/duplicate/
//                  excluded), auto-escalation threshold, and approval-chain requirements per
//                  CrossSiteActionType. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Coordination.Policies;
using PlantSafety.Tests.Common.TestBase;

namespace PlantSafety.Tests.Domain.GlobalSafetyInsights.Coordination;

/// <summary>Unit tests for <see cref="MultiSiteSafetyPolicy"/>.</summary>
public sealed class MultiSiteSafetyPolicyTests : UnitTestBase
{
    private readonly MultiSiteSafetyPolicy _sut = new();

    [Fact]
    public void Evaluate_WithDistinctValidSites_MarksAllEligible()
    {
        var siteA = Guid.NewGuid();
        var siteB = Guid.NewGuid();

        var decision = _sut.Evaluate(TestTenantId.Value, [siteA, siteB], CrossSiteActionType.MaintenanceCoordination);

        decision.IsEligible.Should().BeTrue();
        decision.EligibleSiteIds.Should().BeEquivalentTo([siteA, siteB]);
        decision.IneligibleSites.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WithEmptyGuid_MarksItInvalidSiteId()
    {
        var decision = _sut.Evaluate(TestTenantId.Value, [Guid.Empty], CrossSiteActionType.MaintenanceCoordination);

        decision.IsEligible.Should().BeFalse();
        decision.IneligibleSites.Should().ContainSingle(s => s.Reason == SiteIneligibilityReason.InvalidSiteId);
    }

    [Fact]
    public void Evaluate_WithDuplicateSite_MarksSecondOccurrenceDuplicate()
    {
        var site = Guid.NewGuid();

        var decision = _sut.Evaluate(TestTenantId.Value, [site, site], CrossSiteActionType.MaintenanceCoordination);

        decision.EligibleSiteIds.Should().BeEquivalentTo([site]);
        decision.IneligibleSites.Should().ContainSingle(s => s.Reason == SiteIneligibilityReason.Duplicate);
    }

    [Fact]
    public void Evaluate_WithTenantExcludedSite_MarksItExcluded()
    {
        var eligibleSite = Guid.NewGuid();
        var excludedSite = Guid.NewGuid();

        var decision = _sut.Evaluate(TestTenantId.Value, [eligibleSite, excludedSite], CrossSiteActionType.MaintenanceCoordination, [excludedSite]);

        decision.EligibleSiteIds.Should().BeEquivalentTo([eligibleSite]);
        decision.IneligibleSites.Should().ContainSingle(s => s.SiteId == excludedSite && s.Reason == SiteIneligibilityReason.ExcludedByTenantPolicy);
    }

    [Theory]
    [InlineData(0.5, false)]
    [InlineData(2.9, false)]
    [InlineData(3.0, true)]
    [InlineData(5.0, true)]
    public void ShouldAutoEscalate_ComparesAgainstThreshold(double deviationMagnitude, bool expected)
    {
        _sut.ShouldAutoEscalate(deviationMagnitude).Should().Be(expected);
    }

    [Fact]
    public void Evaluate_CorrectiveActionRollout_AlwaysRequiresApproval()
    {
        var decision = _sut.Evaluate(TestTenantId.Value, [Guid.NewGuid()], CrossSiteActionType.CorrectiveActionRollout);

        decision.RequiresApproval.Should().BeTrue();
        decision.ApprovalReason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Evaluate_LockoutPropagation_AlwaysRequiresApproval()
    {
        var decision = _sut.Evaluate(TestTenantId.Value, [Guid.NewGuid()], CrossSiteActionType.LockoutPropagation);

        decision.RequiresApproval.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_MaintenanceCoordination_AlwaysRequiresApproval()
    {
        var decision = _sut.Evaluate(TestTenantId.Value, [Guid.NewGuid()], CrossSiteActionType.MaintenanceCoordination);

        decision.RequiresApproval.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_IncidentEscalation_WithFewSites_DoesNotRequireApproval()
    {
        var sites = Enumerable.Range(0, MultiSiteSafetyPolicy.ApprovalRequiredSiteCountThreshold).Select(_ => Guid.NewGuid()).ToArray();

        var decision = _sut.Evaluate(TestTenantId.Value, sites, CrossSiteActionType.IncidentEscalation);

        decision.RequiresApproval.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_IncidentEscalation_ExceedingSiteCountThreshold_RequiresApproval()
    {
        var sites = Enumerable.Range(0, MultiSiteSafetyPolicy.ApprovalRequiredSiteCountThreshold + 1).Select(_ => Guid.NewGuid()).ToArray();

        var decision = _sut.Evaluate(TestTenantId.Value, sites, CrossSiteActionType.IncidentEscalation);

        decision.RequiresApproval.Should().BeTrue();
    }
}
