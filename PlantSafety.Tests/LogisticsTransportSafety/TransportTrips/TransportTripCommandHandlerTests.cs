// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportTrips
//  Layer:          Tests (Unit)
//  File:           TransportTripCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for all TransportTrip command handlers: Plan, Complete, Cancel.
//                  Repository is mocked with NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.ValueObjects.Ids;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportTrips;

public sealed class TransportTripCommandHandlerTests : UnitTestBase
{
    private static readonly Guid VehicleId = new("0000000d-0000-0000-0000-00000000000d");
    private static readonly Guid DriverUserId = new("0000000e-0000-0000-0000-00000000000e");
    private readonly ITransportTripRepository _repo = Substitute.For<ITransportTripRepository>();

    private static Agg Planned() =>
        Agg.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleId: VehicleId, driverUserId: DriverUserId,
            routeDescription: "Dock 3 to Warehouse B",
            plannedDepartureAtUtc: UtcTimestamp.From(FixedUtcNow.AddHours(1)),
            plannedArrivalAtUtc: UtcTimestamp.From(FixedUtcNow.AddHours(3)));

    // ---- PlanTransportTripCommandHandler --------------------------------------------

    [Fact]
    public async Task Plan_WithValidCommand_ShouldReturnSuccessAndCallAddAsync()
    {
        var sut = new PlanTransportTripCommandHandler(_repo, NullLogger<PlanTransportTripCommandHandler>());

        var result = await sut.Handle(new PlanTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            VehicleId, DriverUserId, "OffSiteDelivery", "Dock 3 to Warehouse B",
            FixedUtcNow.AddHours(1), FixedUtcNow.AddHours(3), TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Planned");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Plan_WithInvalidTripType_ShouldReturnFailure()
    {
        var sut = new PlanTransportTripCommandHandler(_repo, NullLogger<PlanTransportTripCommandHandler>());

        var result = await sut.Handle(new PlanTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            VehicleId, DriverUserId, "NotARealType", "Dock 3 to Warehouse B",
            FixedUtcNow.AddHours(1), FixedUtcNow.AddHours(3), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportTrips.InvalidTripType");
    }

    // ---- CompleteTransportTripCommandHandler -----------------------------------------

    [Fact]
    public async Task Complete_WhenAggregateExists_ShouldReturnSuccess()
    {
        var trip = Planned();
        _repo.GetByIdAsync(Arg.Any<TransportTripId>(), Arg.Any<CancellationToken>()).Returns(trip);
        var sut = new CompleteTransportTripCommandHandler(_repo, NullLogger<CompleteTransportTripCommandHandler>());

        var result = await sut.Handle(new CompleteTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            trip.TransportTripId.Value, FixedUtcNow, FixedUtcNow.AddHours(2), 42.5m, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Completed");
        await _repo.Received(1).UpdateAsync(trip, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Complete_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<TransportTripId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new CompleteTransportTripCommandHandler(_repo, NullLogger<CompleteTransportTripCommandHandler>());

        var result = await sut.Handle(new CompleteTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), FixedUtcNow, FixedUtcNow.AddHours(2), 42.5m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportTrips.NotFound");
    }

    [Fact]
    public async Task Complete_WithNegativeDistance_ShouldReturnGenericFailure()
    {
        var trip = Planned();
        _repo.GetByIdAsync(Arg.Any<TransportTripId>(), Arg.Any<CancellationToken>()).Returns(trip);
        var sut = new CompleteTransportTripCommandHandler(_repo, NullLogger<CompleteTransportTripCommandHandler>());

        var result = await sut.Handle(new CompleteTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            trip.TransportTripId.Value, FixedUtcNow, FixedUtcNow.AddHours(2), -1m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportTrips.CompleteFailed");
    }

    // ---- CancelTransportTripCommandHandler --------------------------------------------

    [Fact]
    public async Task Cancel_WhenAggregateExists_ShouldReturnSuccess()
    {
        var trip = Planned();
        _repo.GetByIdAsync(Arg.Any<TransportTripId>(), Arg.Any<CancellationToken>()).Returns(trip);
        var sut = new CancelTransportTripCommandHandler(_repo, NullLogger<CancelTransportTripCommandHandler>());

        var result = await sut.Handle(new CancelTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            trip.TransportTripId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Cancelled");
        await _repo.Received(1).UpdateAsync(trip, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancel_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<TransportTripId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new CancelTransportTripCommandHandler(_repo, NullLogger<CancelTransportTripCommandHandler>());

        var result = await sut.Handle(new CancelTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportTrips.NotFound");
    }

    [Fact]
    public async Task Cancel_WhenAlreadyCompleted_ShouldReturnGenericFailure()
    {
        var trip = Planned();
        trip.Complete(FixedTimestamp, UtcTimestamp.From(FixedUtcNow.AddHours(2)), 42.5m, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<TransportTripId>(), Arg.Any<CancellationToken>()).Returns(trip);
        var sut = new CancelTransportTripCommandHandler(_repo, NullLogger<CancelTransportTripCommandHandler>());

        var result = await sut.Handle(new CancelTransportTripCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            trip.TransportTripId.Value, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportTrips.CancelFailed");
    }
}
