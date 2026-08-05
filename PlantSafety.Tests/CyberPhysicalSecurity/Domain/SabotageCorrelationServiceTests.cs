// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Domain
//  Layer:          Tests (Unit)
//  File:           SabotageCorrelationServiceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SabotageCorrelationService -- the module's "correlated
//                  multi-source indicators auto-escalate" risk hook. Also serves as the regression
//                  test for the hand-fixed CS0118 'SabotageIndicator is a namespace but used like a
//                  type' collision (SabotageIndicatorAggregate type alias in
//                  SabotageCorrelationService.cs) -- successful compilation and correct behavior of
//                  this suite is direct proof the fix holds.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Services;
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Domain;

public sealed class SabotageCorrelationServiceTests : UnitTestBase
{
    private static SabotageIndicatorAggregate IndicatorFrom(SabotageIndicatorSource source) =>
        SabotageIndicatorAggregate.Raise(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            source: source, description: "indicator", severity: SabotageIndicatorSeverity.Medium);

    [Fact]
    public void Correlate_TypeAliasRegression_ResolvesSabotageIndicatorAggregateNotNamespace()
    {
        // Regression guard for the hand-fixed CS0118 collision: SabotageCorrelationService.Correlate
        // must accept and return references to the SabotageIndicator *aggregate type*
        // (PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator),
        // not be blocked by the sibling SabotageIndicator *namespace* segment of the same name. If
        // the alias regressed, this file would fail to compile entirely.
        var newIndicator = IndicatorFrom(SabotageIndicatorSource.ControlChange);

        var (correlatedCount, autoEscalate) = SabotageCorrelationService.Correlate(newIndicator, []);

        newIndicator.Should().BeOfType<SabotageIndicatorAggregate>();
        correlatedCount.Should().Be(1);
        autoEscalate.Should().BeFalse();
    }

    [Fact]
    public void Correlate_WithNoCandidates_ReturnsCountOfOneAndNoEscalation()
    {
        var newIndicator = IndicatorFrom(SabotageIndicatorSource.ControlChange);

        var (correlatedCount, autoEscalate) = SabotageCorrelationService.Correlate(newIndicator, []);

        correlatedCount.Should().Be(1);
        autoEscalate.Should().BeFalse();
    }

    [Fact]
    public void Correlate_WithSingleCandidateFromSameSource_DoesNotAutoEscalate()
    {
        var newIndicator = IndicatorFrom(SabotageIndicatorSource.ControlChange);
        var candidate = IndicatorFrom(SabotageIndicatorSource.ControlChange);

        var (correlatedCount, autoEscalate) = SabotageCorrelationService.Correlate(newIndicator, [candidate]);

        correlatedCount.Should().Be(2);
        autoEscalate.Should().BeFalse("both indicators share the same source -- below the distinct-source threshold");
    }

    [Fact]
    public void Correlate_WithCandidateFromDistinctSource_AutoEscalates()
    {
        var newIndicator = IndicatorFrom(SabotageIndicatorSource.ControlChange);
        var candidate = IndicatorFrom(SabotageIndicatorSource.SensorIntegrity);

        var (correlatedCount, autoEscalate) = SabotageCorrelationService.Correlate(newIndicator, [candidate]);

        correlatedCount.Should().Be(2);
        autoEscalate.Should().BeTrue("two distinct sources meet SabotageEscalationRules.AutoEscalationDistinctSourceThreshold");
    }

    [Fact]
    public void Correlate_WithManyCandidatesFromSameSourceAsNew_DoesNotAutoEscalate()
    {
        var newIndicator = IndicatorFrom(SabotageIndicatorSource.Manual);
        var candidates = Enumerable.Range(0, 5).Select(_ => IndicatorFrom(SabotageIndicatorSource.Manual)).ToList();

        var (correlatedCount, autoEscalate) = SabotageCorrelationService.Correlate(newIndicator, candidates);

        correlatedCount.Should().Be(6);
        autoEscalate.Should().BeFalse();
    }

    [Fact]
    public void Correlate_WithThreeDistinctSources_AutoEscalates()
    {
        var newIndicator = IndicatorFrom(SabotageIndicatorSource.ExternalIntel);
        var candidates = new[]
        {
            IndicatorFrom(SabotageIndicatorSource.ControlChange),
            IndicatorFrom(SabotageIndicatorSource.PhysicalAccess),
        };

        var (correlatedCount, autoEscalate) = SabotageCorrelationService.Correlate(newIndicator, candidates);

        correlatedCount.Should().Be(3);
        autoEscalate.Should().BeTrue();
    }
}
