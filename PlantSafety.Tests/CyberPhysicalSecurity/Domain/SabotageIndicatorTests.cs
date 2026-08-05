// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Domain
//  Layer:          Tests (Unit)
//  File:           SabotageIndicatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Functional/lifecycle unit tests for the SabotageIndicator aggregate root
//                  (Sabotage Indicator Correlation subdomain). Covers Raise, ApplyCorrelation
//                  auto-escalation, Confirm/MarkFalsePositive triage, and invalid transitions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Exceptions;
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Domain;

public sealed class SabotageIndicatorTests : UnitTestBase
{
    private static SabotageIndicatorAggregate Raised(
        SabotageIndicatorSource source = SabotageIndicatorSource.ControlChange,
        SabotageIndicatorSeverity severity = SabotageIndicatorSeverity.Medium) =>
        SabotageIndicatorAggregate.Raise(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            source: source,
            description: "Unauthorized control change detected",
            severity: severity);

    [Fact]
    public void Raise_WithValidData_CreatesOpenIndicatorWithCorrelatedCountOfOne()
    {
        var indicator = Raised();

        indicator.Status.Should().Be(SabotageIndicatorStatus.Open);
        indicator.CorrelatedIndicatorCount.Should().Be(1);
        indicator.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SabotageIndicatorRaisedEvent>();
    }

    [Fact]
    public void ApplyCorrelation_WithoutAutoEscalate_UpdatesCountOnlyAndRaisesNoEvent()
    {
        var indicator = Raised();
        indicator.ClearDomainEvents();

        indicator.ApplyCorrelation(2, autoEscalate: false, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        indicator.CorrelatedIndicatorCount.Should().Be(2);
        indicator.Severity.Should().Be(SabotageIndicatorSeverity.Medium, "no escalation should occur");
        indicator.Status.Should().Be(SabotageIndicatorStatus.Open);
        indicator.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ApplyCorrelation_WithAutoEscalate_PromotesToUnderInvestigationAndCritical()
    {
        var indicator = Raised(severity: SabotageIndicatorSeverity.Low);
        indicator.ClearDomainEvents();

        indicator.ApplyCorrelation(3, autoEscalate: true, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        indicator.CorrelatedIndicatorCount.Should().Be(3);
        indicator.Severity.Should().Be(SabotageIndicatorSeverity.Critical);
        indicator.Status.Should().Be(SabotageIndicatorStatus.UnderInvestigation);
        indicator.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SabotageIndicatorEscalatedEvent>();
    }

    [Fact]
    public void Confirm_FromOpen_TransitionsToConfirmed()
    {
        var indicator = Raised();

        indicator.Confirm();

        indicator.Status.Should().Be(SabotageIndicatorStatus.Confirmed);
    }

    [Fact]
    public void Confirm_FromUnderInvestigation_TransitionsToConfirmed()
    {
        var indicator = Raised();
        indicator.ApplyCorrelation(2, autoEscalate: true, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        indicator.Confirm();

        indicator.Status.Should().Be(SabotageIndicatorStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ThrowsDomainException()
    {
        var indicator = Raised();
        indicator.Confirm();

        var act = () => indicator.Confirm();

        act.Should().Throw<SabotageIndicatorDomainException>();
    }

    [Fact]
    public void MarkFalsePositive_FromOpen_TransitionsToFalsePositive()
    {
        var indicator = Raised();

        indicator.MarkFalsePositive();

        indicator.Status.Should().Be(SabotageIndicatorStatus.FalsePositive);
    }

    [Fact]
    public void MarkFalsePositive_WhenAlreadyFalsePositive_ThrowsDomainException()
    {
        var indicator = Raised();
        indicator.MarkFalsePositive();

        var act = () => indicator.MarkFalsePositive();

        act.Should().Throw<SabotageIndicatorDomainException>();
    }

    [Fact]
    public void Confirm_WhenFalsePositive_ThrowsDomainException()
    {
        var indicator = Raised();
        indicator.MarkFalsePositive();

        var act = () => indicator.Confirm();

        act.Should().Throw<SabotageIndicatorDomainException>();
    }
}
