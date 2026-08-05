// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.Carriers
//  Layer:          Tests (Unit)
//  File:           CarrierAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Carrier aggregate root: registration, qualification,
//                  suspension, re-qualification after suspension, and retirement lifecycle, plus
//                  invariant-guard tests for illegal transitions and out-of-range safety scores.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Events;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Exceptions;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.LogisticsTransportSafety.Carriers;

/// <summary>Unit tests for <see cref="Carrier"/> in the LogisticsTransportSafety bounded context.</summary>
public sealed class CarrierAggregateTests : UnitTestBase
{
    private static Carrier CreatePending() =>
        Carrier.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            carrierName: "Acme Freight Lines",
            carrierType: CarrierType.ThirdPartyContractCarrier,
            insuranceExpiryAtUtc: UtcTimestamp.From(FixedUtcNow.AddYears(1)));

    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreatePending().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusPendingQualification()
    {
        var agg = CreatePending();
        agg.Status.Should().Be(CarrierStatus.PendingQualification);
    }

    [Fact]
    public void Create_ShouldTrimCarrierName()
    {
        var agg = Carrier.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, carrierName: "  Beta Transport  ");
        agg.CarrierName.Should().Be("Beta Transport");
    }

    [Fact]
    public void Create_ShouldRaiseCarrierRegisteredEvent()
    {
        var agg = CreatePending();
        agg.DomainEvents.ShouldRaiseSingleEvent<CarrierRegisteredEvent>();
    }

    // ---- Qualify --------------------------------------------------------------

    [Fact]
    public void Qualify_WhenPendingQualification_ShouldTransitionToQualified()
    {
        var agg = CreatePending();
        agg.Qualify(92m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(CarrierStatus.Qualified);
    }

    [Fact]
    public void Qualify_ShouldSetSafetyRatingScoreAndQualifiedAtUtc()
    {
        var agg = CreatePending();
        agg.Qualify(92m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.SafetyRatingScore.Should().Be(92m);
        agg.QualifiedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Qualify_ShouldRaiseEvent()
    {
        var agg = CreatePending();
        agg.ClearDomainEvents();
        agg.Qualify(92m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CarrierQualifiedEvent>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Qualify_WithScoreOutOfRange_ShouldThrowArgumentOutOfRangeException(double score)
    {
        var agg = CreatePending();
        var act = () => agg.Qualify((decimal)score, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Qualify_WhenRetired_ShouldThrowInvalidOperationException()
    {
        var agg = CreatePending();
        agg.Qualify(92m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Qualify(92m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Suspend / re-Qualify ---------------------------------------------------

    private static Carrier CreateQualified()
    {
        var agg = CreatePending();
        agg.Qualify(92m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void Suspend_WhenQualified_ShouldTransitionToSuspended()
    {
        var agg = CreateQualified();
        agg.Suspend("Insurance lapsed", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(CarrierStatus.Suspended);
    }

    [Fact]
    public void Suspend_ShouldSetSuspensionReason()
    {
        var agg = CreateQualified();
        agg.Suspend("  Insurance lapsed  ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.SuspensionReason.Should().Be("Insurance lapsed");
    }

    [Fact]
    public void Suspend_ShouldRaiseEvent()
    {
        var agg = CreateQualified();
        agg.ClearDomainEvents();
        agg.Suspend("Insurance lapsed", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CarrierSuspendedEvent>();
    }

    [Fact]
    public void Suspend_WhenPendingQualification_ShouldThrowInvalidOperationException()
    {
        var agg = CreatePending();
        var act = () => agg.Suspend("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Qualify_AfterSuspension_ShouldClearSuspensionReasonAndTransitionToQualified()
    {
        var agg = CreateQualified();
        agg.Suspend("Insurance lapsed", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Qualify(97m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(CarrierStatus.Qualified);
        agg.SuspensionReason.Should().BeNull();
    }

    // ---- Retire -----------------------------------------------------------------

    [Fact]
    public void Retire_WhenQualified_ShouldTransitionToRetired()
    {
        var agg = CreateQualified();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(CarrierStatus.Retired);
    }

    [Fact]
    public void Retire_ShouldRaiseEvent()
    {
        var agg = CreateQualified();
        agg.ClearDomainEvents();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<CarrierRetiredEvent>();
    }

    [Fact]
    public void Retire_WhenAlreadyRetired_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateQualified();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CarrierInvalidStatusTransitionException>();
    }
}
