// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Handlers
//  Layer:          Tests (Unit)
//  File:           ListActiveObstructionsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListActiveObstructionsQueryHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Handlers;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Handlers;

public sealed class ListActiveObstructionsQueryHandlerTests : UnitTestBase
{
    private readonly IEvacuationRouteRepository _repo = Substitute.For<IEvacuationRouteRepository>();

    private ListActiveObstructionsQueryHandler Sut() => new(_repo, NullLogger<ListActiveObstructionsQueryHandler>());

    private static RouteObstructionItemDto ObstructionFor(Guid tenantId) => new()
    {
        RouteObstructionId = Guid.NewGuid(),
        EvacuationRouteId = Guid.NewGuid(),
        TenantId = tenantId,
        Description = "Blocked corridor",
        Severity = "Moderate",
        RecordedAtUtc = FixedUtcNow,
        ClearedAtUtc = null,
    };

    [Fact]
    public async Task Handle_WithOpenObstructions_ReturnsThem()
    {
        var items = new List<RouteObstructionItemDto> { ObstructionFor(TestTenantId.Value) };
        _repo.ListActiveObstructionsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListActiveObstructionsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.Single().ClearedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithSiteFilter_PassesSiteIdThrough()
    {
        var siteId = Guid.NewGuid();
        _repo.ListActiveObstructionsAsync(TestTenantId.Value, siteId, Arg.Any<CancellationToken>()).Returns(new List<RouteObstructionItemDto>());

        await Sut().Handle(new ListActiveObstructionsQuery(TestTenantId.Value, siteId), CancellationToken.None);

        await _repo.Received(1).ListActiveObstructionsAsync(TestTenantId.Value, siteId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _repo.ListActiveObstructionsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<RouteObstructionItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListActiveObstructionsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.ListActiveObstructionsFailed");
    }
}
