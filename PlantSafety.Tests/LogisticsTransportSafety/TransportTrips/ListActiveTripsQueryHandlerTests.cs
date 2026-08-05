// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportTrips
//  Layer:          Tests (Unit)
//  File:           ListActiveTripsQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListActiveTripsQueryHandler. Read-model repository is mocked
//                  with NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Handlers;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Models;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Repositories;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportTrips;

public sealed class ListActiveTripsQueryHandlerTests : UnitTestBase
{
    private readonly ITransportTripRepository _repo = Substitute.For<ITransportTripRepository>();

    private static TransportTripItemDto Item() => new()
    {
        TransportTripId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        VehicleId = Guid.NewGuid(),
        DriverUserId = Guid.NewGuid(),
        TripType = "OffSiteDelivery",
        TripStatus = "Planned",
        RouteDescription = "Dock 3 to Warehouse B",
        PlannedDepartureAtUtc = FixedUtcNow.AddHours(1),
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingItems_ShouldReturnSuccessWithItems()
    {
        _repo.ListActiveTripsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>())
            .Returns(new List<TransportTripItemDto> { Item() });
        var sut = new ListActiveTripsQueryHandler(_repo, NullLogger<ListActiveTripsQueryHandler>());

        var result = await sut.Handle(new ListActiveTripsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnSuccessWithEmptyItems()
    {
        _repo.ListActiveTripsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new List<TransportTripItemDto>());
        var sut = new ListActiveTripsQueryHandler(_repo, NullLogger<ListActiveTripsQueryHandler>());

        var result = await sut.Handle(new ListActiveTripsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailure()
    {
        _repo.ListActiveTripsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<TransportTripItemDto>>>(_ => throw new InvalidOperationException("DB unavailable"));
        var sut = new ListActiveTripsQueryHandler(_repo, NullLogger<ListActiveTripsQueryHandler>());

        var result = await sut.Handle(new ListActiveTripsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportTrips.RetrievalFailed");
    }
}
