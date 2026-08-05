// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           AccessAnomalyAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the AccessAnomaly aggregate root: detection (type-specific
//                  event dispatch), triage lifecycle (Acknowledge -> Escalate -> Resolve),
//                  invariant-guard tests for illegal transitions and the SeverityScore
//                  0-100 range guard, and domain-event assertions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.PhysicalAccessControl.Domain;

/// <summary>Unit tests for <see cref="AccessAnomaly"/> in the PhysicalAccessControl bounded context.</summary>
public sealed class AccessAnomalyAggregateTests : UnitTestBase
{
    private static AccessAnomaly CreateDetected(
        AccessAnomalyType anomalyType = AccessAnomalyType.UnauthorizedAccess,
        decimal severityScore = 50m,
        Guid? safetyZoneId = null) =>
        AccessAnomaly.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            accessEventId: Guid.NewGuid(),
            accessPointId: Guid.NewGuid(),
            credentialId: Guid.NewGuid(),
            safetyZoneId: safetyZoneId ?? Guid.NewGuid(),
            anomalyType: anomalyType,
            severityScore: severityScore);

    // ---- Create ----------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateDetected().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusDetected()
    {
        var agg = CreateDetected();
        agg.Status.Should().Be(AccessAnomalyStatus.Detected);
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateDetected();
        var a2 = CreateDetected();
        a1.AccessAnomalyId.Should().NotBe(a2.AccessAnomalyId);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    [InlineData(-50)]
    [InlineData(500)]
    public void Create_WithSeverityScoreOutOfRange_ShouldThrowArgumentOutOfRangeException(decimal severityScore)
    {
        var act = () => CreateDetected(severityScore: severityScore);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(50.5)]
    public void Create_WithSeverityScoreInRange_ShouldSucceed(decimal severityScore)
    {
        var agg = CreateDetected(severityScore: severityScore);
        agg.SeverityScore.Should().Be(severityScore);
    }

    [Fact]
    public void Create_WithTailgating_ShouldRaiseTailgatingDetectedEvent()
    {
        var agg = CreateDetected(AccessAnomalyType.Tailgating);
        agg.DomainEvents.ShouldRaiseSingleEvent<TailgatingDetectedEvent>();
    }

    [Fact]
    public void Create_WithAfterHoursAccess_ShouldRaiseAfterHoursAccessDetectedEvent()
    {
        var agg = CreateDetected(AccessAnomalyType.AfterHoursAccess);
        agg.DomainEvents.ShouldRaiseSingleEvent<AfterHoursAccessDetectedEvent>();
    }

    [Fact]
    public void Create_WithRestrictedZoneViolation_ShouldRaiseRestrictedZoneViolationEvent()
    {
        var zoneId = Guid.NewGuid();
        var agg = CreateDetected(AccessAnomalyType.RestrictedZoneViolation, safetyZoneId: zoneId);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<RestrictedZoneViolationEvent>();
        evt.SafetyZoneId.Should().Be(zoneId);
    }

    [Theory]
    [InlineData(AccessAnomalyType.ForcedEntry)]
    [InlineData(AccessAnomalyType.UnauthorizedAccess)]
    public void Create_WithForcedEntryOrUnauthorizedAccess_ShouldRaiseUnauthorizedAccessDetectedEvent(AccessAnomalyType anomalyType)
    {
        var agg = CreateDetected(anomalyType);
        agg.DomainEvents.ShouldRaiseSingleEvent<UnauthorizedAccessDetectedEvent>();
    }

    // ---- Acknowledge -----------------------------------------------------------------

    [Fact]
    public void Acknowledge_WhenDetected_ShouldTransitionToAcknowledged()
    {
        var agg = CreateDetected();
        var ackBy = Guid.NewGuid();
        agg.Acknowledge(ackBy, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccessAnomalyStatus.Acknowledged);
    }

    [Fact]
    public void Acknowledge_ShouldPersistAcknowledgedByUserId()
    {
        var agg = CreateDetected();
        var ackBy = Guid.NewGuid();
        agg.Acknowledge(ackBy, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.AcknowledgedByUserId.Should().Be(ackBy);
    }

    [Fact]
    public void Acknowledge_ShouldRaiseEvent()
    {
        var agg = CreateDetected();
        agg.ClearDomainEvents();
        agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessAnomalyAcknowledgedEvent>();
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ShouldThrowInvalidOperationException()
    {
        var agg = CreateDetected();
        agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Escalate --------------------------------------------------------------------

    [Fact]
    public void Escalate_WhenDetected_ShouldTransitionToEscalated()
    {
        var agg = CreateDetected();
        agg.Escalate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(AccessAnomalyStatus.Escalated);
    }

    [Fact]
    public void Escalate_FromAcknowledged_ShouldTransitionToEscalated()
    {
        var agg = CreateDetected();
        agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Escalate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccessAnomalyStatus.Escalated);
    }

    [Fact]
    public void Escalate_ShouldRaiseEvent()
    {
        var agg = CreateDetected();
        agg.ClearDomainEvents();
        agg.Escalate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<AccessAnomalyEscalatedEvent>();
    }

    [Fact]
    public void Escalate_WhenAlreadyResolved_ShouldThrowInvalidOperationException()
    {
        var agg = CreateDetected();
        agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Resolve("Investigated", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Escalate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Resolve ---------------------------------------------------------------------

    [Fact]
    public void Resolve_FromAcknowledged_ShouldTransitionToResolved()
    {
        var agg = CreateDetected();
        agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Resolve("False positive - authorized after-hours maintenance", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccessAnomalyStatus.Resolved);
    }

    [Fact]
    public void Resolve_FromEscalated_ShouldTransitionToResolved()
    {
        var agg = CreateDetected();
        agg.Escalate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Resolve("Investigated and closed", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(AccessAnomalyStatus.Resolved);
    }

    [Fact]
    public void Resolve_ShouldTrimAndPersistResolution()
    {
        var agg = CreateDetected();
        agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Resolve("  Confirmed benign  ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Resolution.Should().Be("Confirmed benign");
    }

    [Fact]
    public void Resolve_ShouldRaiseEvent()
    {
        var agg = CreateDetected();
        agg.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();

        agg.Resolve("Resolved", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.DomainEvents.ShouldRaiseSingleEvent<AccessAnomalyResolvedEvent>();
    }

    [Fact]
    public void Resolve_WhenStillDetected_ShouldThrowInvalidOperationException()
    {
        var agg = CreateDetected();
        var act = () => agg.Resolve("Too early", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }
}
