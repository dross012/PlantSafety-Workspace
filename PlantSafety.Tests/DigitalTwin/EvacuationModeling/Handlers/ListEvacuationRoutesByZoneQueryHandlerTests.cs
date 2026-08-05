// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Handlers
//  Layer:          Tests (Unit)
//  File:           ListEvacuationRoutesByZoneQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListEvacuationRoutesByZoneQueryHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Handlers;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Handlers;

public sealed class ListEvacuationRoutesByZoneQueryHandlerTests : UnitTestBase
{
    private readonly IEvacuationRouteRepository _repo = Substitute.For<IEvacuationRouteRepository>();

    private ListEvacuationRoutesByZoneQueryHandler Sut() => new(_repo, NullLogger<ListEvacuationRoutesByZoneQueryHandler>());

    private static EvacuationRouteItemDto RouteFor(Guid zoneId) => new()
    {
        EvacuationRouteId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        RouteName = "Corridor A",
        ZoneId = zoneId,
        AssemblyPointId = Guid.NewGuid(),
        PathGeometryRef = "geo://1",
        TwinModelVersion = "v2026.1.0",
        EstimatedEgressTimeSeconds = 120,
        ObstructionCount = 0,
        RouteStatus = "Active",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingRoutes_ReturnsAllOfThem()
    {
        var zoneId = Guid.NewGuid();
        var routes = new List<EvacuationRouteItemDto> { RouteFor(zoneId), RouteFor(zoneId) };
        _repo.ListByZoneAsync(TestTenantId.Value, zoneId, Arg.Any<CancellationToken>()).Returns(routes);

        var result = await Sut().Handle(new ListEvacuationRoutesByZoneQuery(TestTenantId.Value, zoneId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMatchingRoutes_ReturnsEmptyList()
    {
        _repo.ListByZoneAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(new List<EvacuationRouteItemDto>());

        var result = await Sut().Handle(new ListEvacuationRoutesByZoneQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _repo.ListByZoneAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<EvacuationRouteItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListEvacuationRoutesByZoneQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.ListByZoneFailed");
    }
}
