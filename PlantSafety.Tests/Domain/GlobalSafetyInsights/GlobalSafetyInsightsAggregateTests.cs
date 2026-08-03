// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.GlobalSafetyInsights
//  Layer:          Tests (Unit)
//  File:           GlobalSafetyInsightsAggregateTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GlobalSafetyInsightsAggregate.Compute: invariant enforcement and
//                  domain-event raising (insight-computed/risk-scored/anomaly-detected). Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Enums;
using PlantSafety.Domain.GlobalSafetyInsights.Events;
using PlantSafety.Domain.GlobalSafetyInsights.Exceptions;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using PlantSafety.Tests.Common.TestBase;

namespace PlantSafety.Tests.Domain.GlobalSafetyInsights;

/// <summary>Unit tests for <see cref="GlobalSafetyInsightsAggregate"/>.</summary>
public sealed class GlobalSafetyInsightsAggregateTests : UnitTestBase
{
    private static readonly GlobalSafetyKpiSnapshot ZeroKpis = new()
    {
        Trir = 0m,
        Ltir = 0m,
        ExposureRate = 0m,
        IncidentDensity = 0m,
        MachineStateIncidentCorrelation = 0.0,
        TotalRecordableIncidents = 0,
        TotalLostTimeIncidents = 0,
        TotalHoursWorked = 0m,
    };

    private static readonly SafetyAnalysisTimeWindow Window = new(
        UtcTimestamp.From(FixedUtcNow.AddDays(-1)), FixedTimestamp);

    private GlobalSafetyInsightsAggregate Compute(
        IReadOnlyList<Guid>? siteIds = null,
        IReadOnlyList<SiteRiskScore>? riskScores = null,
        IReadOnlyList<DetectedAnomaly>? anomalies = null,
        IReadOnlyList<SafetyRecommendation>? recommendations = null)
        => GlobalSafetyInsightsAggregate.Compute(
            TestTenantId.Value, Region.UsEast, siteIds ?? [Guid.NewGuid()], Window, ZeroKpis,
            riskScores ?? [], anomalies ?? [], recommendations ?? [],
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);

    [Fact]
    public void Compute_ShouldReturnAggregate() => Compute().Should().NotBeNull();

    [Fact]
    public void Compute_ShouldRaiseGlobalSafetyInsightComputedEvent()
    {
        var agg = Compute();
        agg.DomainEvents.Should().ContainSingle(e => e is GlobalSafetyInsightComputedEvent);
    }

    [Fact]
    public void Compute_WithEmptySiteIds_ThrowsInvalidComputationException()
    {
        var act = () => Compute(siteIds: []);
        act.Should().Throw<GlobalSafetyInsightsInvalidComputationException>();
    }

    [Fact]
    public void Compute_WithAnomalies_RaisesOneAnomalyDetectedEventPerAnomaly()
    {
        var anomalies = new[]
        {
            new DetectedAnomaly { Kind = GlobalSafetyAnomalyKind.CrossSiteIncidentSpike, DeviationMagnitude = 3.0, Message = "test", DetectedAtUtc = FixedTimestamp },
            new DetectedAnomaly { Kind = GlobalSafetyAnomalyKind.RiskScoreDrift, DeviationMagnitude = 2.0, Message = "test2", DetectedAtUtc = FixedTimestamp },
        };

        var agg = Compute(anomalies: anomalies);

        agg.DomainEvents.Count(e => e is GlobalSafetyAnomalyDetectedEvent).Should().Be(2);
        agg.Anomalies.Should().HaveCount(2);
    }

    [Fact]
    public void Compute_WithRiskScores_RaisesOneRiskScoredEventPerScore()
    {
        var scores = new[]
        {
            new SiteRiskScore
            {
                ScopeType = GlobalSafetyRiskScopeType.Site,
                ScopeKey = Guid.NewGuid().ToString(),
                AggregatedScore = new PlantSafety.Domain.RiskManagement.ValueObjects.RiskScore(3, 3),
                SourceRiskCount = 1,
            },
        };

        var agg = Compute(riskScores: scores);

        agg.DomainEvents.Count(e => e is GlobalSafetyRiskScoredEvent).Should().Be(1);
        agg.RiskScores.Should().HaveCount(1);
    }

    [Fact]
    public void Compute_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = Compute();
        var a2 = Compute();
        a1.GlobalSafetyInsightsId.Should().NotBe(a2.GlobalSafetyInsightsId);
    }

    [Fact]
    public void RecordCrossSiteAction_AddsHistoryEntry()
    {
        var agg = Compute();
        agg.RecordCrossSiteAction("CorrectiveActionRollout", "Rolling out fix", TestUserId.Value, FixedTimestamp);

        agg.HistoryEntries.Should().Contain(h => h.EntryType == "CrossSiteAction");
    }
}
