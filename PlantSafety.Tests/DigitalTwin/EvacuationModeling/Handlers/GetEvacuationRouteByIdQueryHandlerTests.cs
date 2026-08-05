// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Handlers
//  Layer:          Tests (Unit)
//  File:           GetEvacuationRouteByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetEvacuationRouteByIdQueryHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Handlers;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Handlers;

public sealed class GetEvacuationRouteByIdQueryHandlerTests : UnitTestBase
{
    private readonly IEvacuationRouteRepository _repo = Substitute.For<IEvacuationRouteRepository>();

    private GetEvacuationRouteByIdQueryHandler Sut() => new(_repo, NullLogger<GetEvacuationRouteByIdQueryHandler>());

    private static EvacuationRouteItemDto RouteFor(Guid id, Guid tenantId) => new()
    {
        EvacuationRouteId = id,
        TenantId = tenantId,
        RouteName = "Corridor A",
        ZoneId = Guid.NewGuid(),
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
    public async Task Handle_WhenRouteExists_ReturnsSuccessWithItem()
    {
        var routeId = Guid.NewGuid();
        _repo.GetByIdAsync(TestTenantId.Value, routeId, Arg.Any<CancellationToken>()).Returns(RouteFor(routeId, TestTenantId.Value));

        var result = await Sut().Handle(new GetEvacuationRouteByIdQuery(TestTenantId.Value, routeId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EvacuationRouteId.Should().Be(routeId);
    }

    [Fact]
    public async Task Handle_WhenRouteDoesNotExist_ReturnsSuccessWithNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EvacuationRouteItemDto?)null);

        var result = await Sut().Handle(new GetEvacuationRouteByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<EvacuationRouteItemDto?>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetEvacuationRouteByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.RetrievalFailed");
    }
}
