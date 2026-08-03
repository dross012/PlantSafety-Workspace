// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.GlobalSafetyInsights
//  Layer:          Tests (Unit)
//  File:           GlobalSafetyRiskScorerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GlobalSafetyRiskScorer: composes real RiskManagement.Aggregates.
//                  Risk records (Severity x Likelihood RiskScore) into per-site and region roll-ups.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Services;
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using PlantSafety.Tests.Common.TestBase;

namespace PlantSafety.Tests.Domain.GlobalSafetyInsights;

/// <summary>Unit tests for <see cref="GlobalSafetyRiskScorer"/>.</summary>
public sealed class GlobalSafetyRiskScorerTests : UnitTestBase
{
    private readonly GlobalSafetyRiskScorer _sut = new();

    private static Risk ScoredRisk(Guid siteId, int severity, int likelihood)
    {
        var risk = Risk.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, siteId: siteId, riskTitle: "Test hazard", ownerId: TestUserId);

        risk.ScoreRisk(severity, likelihood, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return risk;
    }

    [Fact]
    public void ScoreBySite_WithNoScoredRisks_ReturnsEmpty()
    {
        var result = _sut.ScoreBySite([], Region.UsEast);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ScoreBySite_SingleSite_ProducesOneSiteScoreAndOneRegionScore()
    {
        var siteId = Guid.NewGuid();
        var risks = new[] { ScoredRisk(siteId, 4, 4) };

        var result = _sut.ScoreBySite(risks, Region.UsEast);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(s => s.ScopeType == GlobalSafetyRiskScopeType.Site && s.ScopeKey == siteId.ToString());
        result.Should().ContainSingle(s => s.ScopeType == GlobalSafetyRiskScopeType.Region && s.ScopeKey == Region.UsEast.Code);
    }

    [Fact]
    public void ScoreBySite_AveragesMultipleRisksAtSameSite()
    {
        var siteId = Guid.NewGuid();
        // Severity 2x Likelihood 2 and Severity 4x Likelihood 4 -> average Severity 3, Likelihood 3.
        var risks = new[] { ScoredRisk(siteId, 2, 2), ScoredRisk(siteId, 4, 4) };

        var result = _sut.ScoreBySite(risks, Region.UsEast);

        var siteScore = result.Single(s => s.ScopeType == GlobalSafetyRiskScopeType.Site);
        siteScore.AggregatedScore.Severity.Should().Be(3);
        siteScore.AggregatedScore.Likelihood.Should().Be(3);
        siteScore.SourceRiskCount.Should().Be(2);
    }

    [Fact]
    public void ScoreBySite_MultipleSites_ProducesOneScorePerSitePlusOneRegionRollup()
    {
        var siteA = Guid.NewGuid();
        var siteB = Guid.NewGuid();
        var risks = new[] { ScoredRisk(siteA, 5, 5), ScoredRisk(siteB, 1, 1) };

        var result = _sut.ScoreBySite(risks, Region.Eu);

        result.Should().HaveCount(3);
        result.Count(s => s.ScopeType == GlobalSafetyRiskScopeType.Site).Should().Be(2);
        result.Count(s => s.ScopeType == GlobalSafetyRiskScopeType.Region).Should().Be(1);
    }

    [Fact]
    public void ScoreBySite_CriticalRisk_BandsAsCriticalLevel()
    {
        var siteId = Guid.NewGuid();
        var risks = new[] { ScoredRisk(siteId, 5, 5) };

        var result = _sut.ScoreBySite(risks, Region.UsEast);

        var siteScore = result.Single(s => s.ScopeType == GlobalSafetyRiskScopeType.Site);
        siteScore.Level.Code.Should().Be("Critical");
    }
}
