// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Domain
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalIncidentFactoryTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CyberPhysicalIncidentFactory.CreateFromEscalatedIndicators --
//                  the Incident Response subdomain's downstream step of the "correlated multi-source
//                  indicators auto-escalate" risk hook. Also serves as the regression test for the
//                  hand-fixed CS0118 'SabotageIndicator is a namespace but used like a type'
//                  collision (SabotageIndicatorAggregate type alias in CyberPhysicalIncidentFactory.cs).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Factories;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Domain;

public sealed class CyberPhysicalIncidentFactoryTests : UnitTestBase
{
    private readonly CyberPhysicalIncidentFactory _sut = new();

    private static SabotageIndicatorAggregate IndicatorFrom(SabotageIndicatorSource source, SabotageIndicatorSeverity severity) =>
        SabotageIndicatorAggregate.Raise(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            source: source, description: $"{source} indicator", severity: severity);

    [Fact]
    public void CreateFromEscalatedIndicators_TypeAliasRegression_AcceptsSabotageIndicatorAggregateCollection()
    {
        // Regression guard for the hand-fixed CS0118 collision in CyberPhysicalIncidentFactory.cs:
        // the factory's parameter type must resolve to the SabotageIndicator aggregate, not be
        // shadowed by the SabotageIndicator namespace segment. A regression here would be a
        // compile-time failure for this entire test file.
        var indicators = new[] { IndicatorFrom(SabotageIndicatorSource.ControlChange, SabotageIndicatorSeverity.High) };

        var incident = _sut.CreateFromEscalatedIndicators(TestTenantId.Value, Guid.NewGuid(), TestUserId, TestUserId, indicators);

        incident.Should().NotBeNull();
    }

    [Fact]
    public void CreateFromEscalatedIndicators_WithNoCriticalIndicators_ResultsInHighSeverityIncident()
    {
        var indicators = new[]
        {
            IndicatorFrom(SabotageIndicatorSource.ControlChange, SabotageIndicatorSeverity.High),
            IndicatorFrom(SabotageIndicatorSource.SensorIntegrity, SabotageIndicatorSeverity.Medium),
        };

        var incident = _sut.CreateFromEscalatedIndicators(TestTenantId.Value, Guid.NewGuid(), TestUserId, TestUserId, indicators);

        incident.Severity.Should().Be(CyberPhysicalIncidentSeverity.High);
    }

    [Fact]
    public void CreateFromEscalatedIndicators_WithAnyCriticalIndicator_ResultsInCriticalSeverityIncident()
    {
        var indicators = new[]
        {
            IndicatorFrom(SabotageIndicatorSource.ControlChange, SabotageIndicatorSeverity.Medium),
            IndicatorFrom(SabotageIndicatorSource.SensorIntegrity, SabotageIndicatorSeverity.Critical),
        };

        var incident = _sut.CreateFromEscalatedIndicators(TestTenantId.Value, Guid.NewGuid(), TestUserId, TestUserId, indicators);

        incident.Severity.Should().Be(CyberPhysicalIncidentSeverity.Critical);
    }

    [Fact]
    public void CreateFromEscalatedIndicators_PopulatesRelatedSabotageIndicatorIds()
    {
        var indicator1 = IndicatorFrom(SabotageIndicatorSource.ControlChange, SabotageIndicatorSeverity.High);
        var indicator2 = IndicatorFrom(SabotageIndicatorSource.SensorIntegrity, SabotageIndicatorSeverity.High);

        var incident = _sut.CreateFromEscalatedIndicators(TestTenantId.Value, Guid.NewGuid(), TestUserId, TestUserId, [indicator1, indicator2]);

        incident.RelatedSabotageIndicatorIds.Should().BeEquivalentTo(
            [indicator1.SabotageIndicatorId.Value, indicator2.SabotageIndicatorId.Value]);
    }

    [Fact]
    public void CreateFromEscalatedIndicators_TitleIncludesDistinctSourceSummary()
    {
        var indicators = new[]
        {
            IndicatorFrom(SabotageIndicatorSource.ControlChange, SabotageIndicatorSeverity.High),
            IndicatorFrom(SabotageIndicatorSource.SensorIntegrity, SabotageIndicatorSeverity.High),
        };

        var incident = _sut.CreateFromEscalatedIndicators(TestTenantId.Value, Guid.NewGuid(), TestUserId, TestUserId, indicators);

        incident.Title.Should().Contain("ControlChange").And.Contain("SensorIntegrity");
    }

    [Fact]
    public void CreateFromEscalatedIndicators_WithEmptyCollection_ThrowsArgumentException()
    {
        var act = () => _sut.CreateFromEscalatedIndicators(TestTenantId.Value, Guid.NewGuid(), TestUserId, TestUserId, []);

        act.Should().Throw<ArgumentException>();
    }
}
