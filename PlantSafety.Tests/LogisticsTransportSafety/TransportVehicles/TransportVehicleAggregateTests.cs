// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportVehicles
//  Layer:          Tests (Unit)
//  File:           TransportVehicleAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the TransportVehicle aggregate root: registration, inspection
//                  scheduling, inspection result recording (pass/fail), and retirement lifecycle,
//                  plus invariant-guard tests for illegal transitions and out-of-range scores.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportVehicles;

/// <summary>Unit tests for <see cref="TransportVehicle"/> in the LogisticsTransportSafety bounded context.</summary>
public sealed class TransportVehicleAggregateTests : UnitTestBase
{
    private static TransportVehicle CreateRegistered() =>
        TransportVehicle.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            vehicleTitle: "Forklift 12",
            vehicleType: TransportVehicleType.Forklift,
            vehicleIdentifier: "FLT-012");

    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateRegistered().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusRegistered()
    {
        var agg = CreateRegistered();
        agg.Status.Should().Be(TransportVehicleStatus.Registered);
    }

    [Fact]
    public void Create_ShouldTrimVehicleTitleAndIdentifier()
    {
        var agg = TransportVehicle.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleTitle: "  Truck 3  ", vehicleIdentifier: "  TRK-3  ");

        agg.VehicleTitle.Should().Be("Truck 3");
        agg.VehicleIdentifier.Should().Be("TRK-3");
    }

    [Fact]
    public void Create_ShouldRaiseTransportVehicleRegisteredEvent()
    {
        var agg = CreateRegistered();
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportVehicleRegisteredEvent>();
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateRegistered();
        var a2 = CreateRegistered();
        a1.TransportVehicleId.Should().NotBe(a2.TransportVehicleId);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmpty()
    {
        var agg = CreateRegistered();
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    // ---- ScheduleInspection ----------------------------------------------------

    [Fact]
    public void ScheduleInspection_WhenRegistered_ShouldTransitionToInspectionScheduled()
    {
        var agg = CreateRegistered();
        agg.ScheduleInspection(UtcTimestamp.From(FixedUtcNow.AddDays(7)), TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportVehicleStatus.InspectionScheduled);
    }

    [Fact]
    public void ScheduleInspection_ShouldSetNextInspectionDueAtUtc()
    {
        var agg = CreateRegistered();
        var due = UtcTimestamp.From(FixedUtcNow.AddDays(7));
        agg.ScheduleInspection(due, TestActor, TestCorrelationId, TestCausationId);
        agg.NextInspectionDueAtUtc.Should().Be(due);
    }

    [Fact]
    public void ScheduleInspection_ShouldRaiseEvent()
    {
        var agg = CreateRegistered();
        agg.ClearDomainEvents();
        agg.ScheduleInspection(UtcTimestamp.From(FixedUtcNow.AddDays(7)), TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportVehicleInspectionScheduledEvent>();
    }

    [Fact]
    public void ScheduleInspection_WhenRetired_ShouldThrowInvalidOperationException()
    {
        var agg = CreateRegistered();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.ScheduleInspection(UtcTimestamp.From(FixedUtcNow.AddDays(7)), TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- RecordInspectionResult -------------------------------------------------

    private static TransportVehicle CreateInspectionScheduled()
    {
        var agg = CreateRegistered();
        agg.ScheduleInspection(UtcTimestamp.From(FixedUtcNow.AddDays(7)), TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void RecordInspectionResult_WhenPassed_ShouldTransitionToInspectionPassed()
    {
        var agg = CreateInspectionScheduled();
        agg.RecordInspectionResult(true, 95m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportVehicleStatus.InspectionPassed);
    }

    [Fact]
    public void RecordInspectionResult_WhenFailed_ShouldTransitionToOutOfService()
    {
        var agg = CreateInspectionScheduled();
        agg.RecordInspectionResult(false, 40m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportVehicleStatus.OutOfService);
    }

    [Fact]
    public void RecordInspectionResult_ShouldSetLastInspectionScoreAndTimestamp()
    {
        var agg = CreateInspectionScheduled();
        agg.RecordInspectionResult(true, 88m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.LastInspectionScore.Should().Be(88m);
        agg.LastInspectionAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void RecordInspectionResult_ShouldRaiseEvent()
    {
        var agg = CreateInspectionScheduled();
        agg.ClearDomainEvents();
        agg.RecordInspectionResult(true, 88m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportVehicleInspectionRecordedEvent>();
    }

    [Fact]
    public void RecordInspectionResult_WhenNotScheduled_ShouldThrowInvalidOperationException()
    {
        var agg = CreateRegistered();
        var act = () => agg.RecordInspectionResult(true, 90m, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void RecordInspectionResult_WithScoreOutOfRange_ShouldThrowArgumentOutOfRangeException(double score)
    {
        var agg = CreateInspectionScheduled();
        var act = () => agg.RecordInspectionResult(true, (decimal)score, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ---- Retire -----------------------------------------------------------------

    [Fact]
    public void Retire_WhenRegistered_ShouldTransitionToRetired()
    {
        var agg = CreateRegistered();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportVehicleStatus.Retired);
    }

    [Fact]
    public void Retire_ShouldRaiseEvent()
    {
        var agg = CreateRegistered();
        agg.ClearDomainEvents();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportVehicleRetiredEvent>();
    }

    [Fact]
    public void Retire_WhenAlreadyRetired_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateRegistered();
        agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Exceptions.TransportVehicleInvalidStatusTransitionException>();
    }
}
