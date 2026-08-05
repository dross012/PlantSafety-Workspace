// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportTrips
//  Layer:          Tests (Unit)
//  File:           TransportTripAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the TransportTrip aggregate root: planning, completion (actual
//                  departure/arrival and distance), and cancellation lifecycle, plus invariant-guard
//                  tests for illegal transitions and negative distance.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Events;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Exceptions;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportTrips;

/// <summary>Unit tests for <see cref="TransportTrip"/> in the LogisticsTransportSafety bounded context.</summary>
public sealed class TransportTripAggregateTests : UnitTestBase
{
    private static readonly Guid VehicleId = new("00000006-0000-0000-0000-000000000006");
    private static readonly Guid DriverUserId = new("00000007-0000-0000-0000-000000000007");

    private static TransportTrip CreatePlanned() =>
        TransportTrip.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            vehicleId: VehicleId,
            driverUserId: DriverUserId,
            tripType: TransportTripType.OffSiteDelivery,
            routeDescription: "Dock 3 to Warehouse B",
            plannedDepartureAtUtc: UtcTimestamp.From(FixedUtcNow.AddHours(1)),
            plannedArrivalAtUtc: UtcTimestamp.From(FixedUtcNow.AddHours(3)));

    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreatePlanned().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusPlanned()
    {
        var agg = CreatePlanned();
        agg.Status.Should().Be(TransportTripStatus.Planned);
    }

    [Fact]
    public void Create_ShouldTrimRouteDescription()
    {
        var agg = TransportTrip.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleId: VehicleId, driverUserId: DriverUserId, routeDescription: "  Route A  ");
        agg.RouteDescription.Should().Be("Route A");
    }

    [Fact]
    public void Create_ShouldRaiseTransportTripPlannedEvent()
    {
        var agg = CreatePlanned();
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportTripPlannedEvent>();
    }

    // ---- Complete ---------------------------------------------------------------

    [Fact]
    public void Complete_WhenPlanned_ShouldTransitionToCompleted()
    {
        var agg = CreatePlanned();
        agg.Complete(FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)), 42.5m, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportTripStatus.Completed);
    }

    [Fact]
    public void Complete_ShouldSetActualDepartureArrivalAndDistance()
    {
        var agg = CreatePlanned();
        var departure = FixedTimestamp;
        var arrival = UtcTimestamp.From(FixedUtcNow.AddHours(2));
        agg.Complete(departure, arrival, 42.5m, TestActor, TestCorrelationId, TestCausationId);

        agg.ActualDepartureAtUtc.Should().Be(departure);
        agg.ActualArrivalAtUtc.Should().Be(arrival);
        agg.DistanceMiles.Should().Be(42.5m);
    }

    [Fact]
    public void Complete_ShouldRaiseEvent()
    {
        var agg = CreatePlanned();
        agg.ClearDomainEvents();
        agg.Complete(FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)), 42.5m, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportTripCompletedEvent>();
    }

    [Fact]
    public void Complete_WithNegativeDistance_ShouldThrowArgumentOutOfRangeException()
    {
        var agg = CreatePlanned();
        var act = () => agg.Complete(FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)), -1m, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreatePlanned();
        agg.Complete(FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)), 42.5m, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Complete(FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)), 42.5m, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<TransportTripInvalidStatusTransitionException>();
    }

    // ---- Cancel -------------------------------------------------------------------

    [Fact]
    public void Cancel_WhenPlanned_ShouldTransitionToCancelled()
    {
        var agg = CreatePlanned();
        agg.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportTripStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldRaiseEvent()
    {
        var agg = CreatePlanned();
        agg.ClearDomainEvents();
        agg.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportTripCancelledEvent>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCompleted_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreatePlanned();
        agg.Complete(FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)), 42.5m, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<TransportTripInvalidStatusTransitionException>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreatePlanned();
        agg.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<TransportTripInvalidStatusTransitionException>();
    }
}
