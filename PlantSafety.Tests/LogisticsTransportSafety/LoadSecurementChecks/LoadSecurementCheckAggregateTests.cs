// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Tests (Unit)
//  File:           LoadSecurementCheckAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the LoadSecurementCheck aggregate root: recording (pass/fail
//                  outcome, including the compound LoadSecurementFailedEvent raised only on
//                  failure) and corrective action completion, plus invariant-guard tests for
//                  illegal transitions and out-of-range securement scores.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Events;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Exceptions;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.LogisticsTransportSafety.LoadSecurementChecks;

/// <summary>Unit tests for <see cref="LoadSecurementCheck"/> in the LogisticsTransportSafety bounded context.</summary>
public sealed class LoadSecurementCheckAggregateTests : UnitTestBase
{
    private static readonly Guid VehicleId = new("00000008-0000-0000-0000-000000000008");
    private static readonly Guid CheckedByUserId = new("00000009-0000-0000-0000-000000000009");

    private static LoadSecurementCheck CreatePassed() =>
        LoadSecurementCheck.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            vehicleId: VehicleId,
            checkedByUserId: CheckedByUserId,
            passed: true,
            deficiencyCount: 0,
            securementScore: 100m);

    private static LoadSecurementCheck CreateFailed() =>
        LoadSecurementCheck.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            vehicleId: VehicleId,
            checkedByUserId: CheckedByUserId,
            passed: false,
            deficiencyCount: 3,
            securementScore: 55m);

    // ---- Create: passed ---------------------------------------------------------

    [Fact]
    public void Create_WhenPassed_ShouldReturnAggregate() =>
        CreatePassed().Should().NotBeNull();

    [Fact]
    public void Create_WhenPassed_ShouldSetStatusPassed()
    {
        var agg = CreatePassed();
        agg.Status.Should().Be(LoadSecurementCheckStatus.Passed);
    }

    [Fact]
    public void Create_WhenPassed_ShouldRaiseOnlyRecordedEvent()
    {
        var agg = CreatePassed();
        agg.DomainEvents.Should().ContainSingle();
        agg.DomainEvents.ShouldRaiseSingleEvent<LoadSecurementCheckRecordedEvent>();
    }

    [Fact]
    public void Create_WhenPassed_ShouldNotRaiseFailedEvent()
    {
        var agg = CreatePassed();
        agg.DomainEvents.Should().NotContain(e => e is LoadSecurementFailedEvent);
    }

    // ---- Create: failed -----------------------------------------------------------

    [Fact]
    public void Create_WhenFailed_ShouldSetStatusFailed()
    {
        var agg = CreateFailed();
        agg.Status.Should().Be(LoadSecurementCheckStatus.Failed);
    }

    [Fact]
    public void Create_WhenFailed_ShouldRaiseBothRecordedAndFailedEvents()
    {
        var agg = CreateFailed();
        agg.DomainEvents.Should().HaveCount(2);
        agg.DomainEvents.ShouldRaiseSingleEvent<LoadSecurementCheckRecordedEvent>();
        var failedEvt = agg.DomainEvents.ShouldRaiseSingleEvent<LoadSecurementFailedEvent>();
        failedEvt.DeficiencyCount.Should().Be(3);
    }

    [Fact]
    public void Create_ShouldSetDeficiencyCountAndSecurementScore()
    {
        var agg = CreateFailed();
        agg.DeficiencyCount.Should().Be(3);
        agg.SecurementScore.Should().Be(55m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Create_WithScoreOutOfRange_ShouldThrowArgumentOutOfRangeException(double score)
    {
        var act = () => LoadSecurementCheck.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleId: VehicleId, checkedByUserId: CheckedByUserId,
            securementScore: (decimal)score);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ---- CompleteCorrectiveAction --------------------------------------------------

    [Fact]
    public void CompleteCorrectiveAction_WhenFailed_ShouldTransitionToCorrectiveActionCompleted()
    {
        var agg = CreateFailed();
        agg.CompleteCorrectiveAction(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(LoadSecurementCheckStatus.CorrectiveActionCompleted);
    }

    [Fact]
    public void CompleteCorrectiveAction_ShouldRaiseEvent()
    {
        var agg = CreateFailed();
        agg.ClearDomainEvents();
        agg.CompleteCorrectiveAction(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<LoadSecurementCorrectiveActionCompletedEvent>();
    }

    [Fact]
    public void CompleteCorrectiveAction_WhenPassed_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreatePassed();
        var act = () => agg.CompleteCorrectiveAction(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<LoadSecurementCheckInvalidStatusTransitionException>();
    }

    [Fact]
    public void CompleteCorrectiveAction_WhenAlreadyCompleted_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateFailed();
        agg.CompleteCorrectiveAction(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.CompleteCorrectiveAction(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LoadSecurementCheckInvalidStatusTransitionException>();
    }
}
