// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Handlers
//  Layer:          Tests (Unit)
//  File:           GetBottleneckHistoryQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetBottleneckHistoryQueryHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Handlers;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Models;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Queries;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Repositories;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Handlers;

public sealed class GetBottleneckHistoryQueryHandlerTests : UnitTestBase
{
    private readonly IEvacuationBottleneckRepository _repo = Substitute.For<IEvacuationBottleneckRepository>();

    private GetBottleneckHistoryQueryHandler Sut() => new(_repo, NullLogger<GetBottleneckHistoryQueryHandler>());

    private static EvacuationBottleneckItemDto BottleneckFor(Guid routeId) => new()
    {
        EvacuationBottleneckId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        RouteId = routeId,
        SimulationRunId = Guid.NewGuid(),
        BottleneckLocation = "Stairwell B",
        DelaySeconds = 120,
        RecurrenceCount = 0,
        BottleneckStatus = "Detected",
        DetectedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithRouteFilter_ReturnsScopedHistory()
    {
        var routeId = Guid.NewGuid();
        _repo.GetHistoryAsync(TestTenantId.Value, null, routeId, Arg.Any<CancellationToken>())
            .Returns(new List<EvacuationBottleneckItemDto> { BottleneckFor(routeId) });

        var result = await Sut().Handle(new GetBottleneckHistoryQuery(TestTenantId.Value, null, routeId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(b => b.RouteId == routeId);
    }

    [Fact]
    public async Task Handle_WithoutRouteFilter_ReturnsAllHistory()
    {
        _repo.GetHistoryAsync(TestTenantId.Value, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<EvacuationBottleneckItemDto> { BottleneckFor(Guid.NewGuid()), BottleneckFor(Guid.NewGuid()) });

        var result = await Sut().Handle(new GetBottleneckHistoryQuery(TestTenantId.Value, null, null), CancellationToken.None);

        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _repo.GetHistoryAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<EvacuationBottleneckItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetBottleneckHistoryQuery(TestTenantId.Value, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationBottleneck.HistoryRetrievalFailed");
    }
}
