// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           ClearRouteObstructionCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ClearRouteObstructionCommandHandler. Extra-careful regression
//                  coverage around the previously hand-reconciled obstruction-clearing flow and
//                  the RouteObstructionEntry.WithCleared() record-with-expression.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class ClearRouteObstructionCommandHandlerTests : UnitTestBase
{
    private readonly IEvacuationRouteRepository _repo = Substitute.For<IEvacuationRouteRepository>();

    private ClearRouteObstructionCommandHandler Sut() => new(_repo, NullLogger<ClearRouteObstructionCommandHandler>());

    private static EvacuationRoute CreatedRouteWithObstruction(out RouteObstructionId obstructionId)
    {
        var route = EvacuationRoute.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0");

        obstructionId = RouteObstructionId.New();
        route.RecordObstruction(obstructionId, "Blocked door", PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums.ObstructionSeverity.Moderate,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return route;
    }

    [Fact]
    public async Task Handle_WithOpenObstruction_ReturnsSuccessAndClearsIt()
    {
        var route = CreatedRouteWithObstruction(out var obstructionId);
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var command = new ClearRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            route.EvacuationRouteId.Value, obstructionId.Value, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
        route.ObstructionHistory.Single().ClearedAtUtc.Should().NotBeNull();
        await _repo.Received(1).UpdateAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRouteNotFound_ReturnsFailureWithNotFoundCode()
    {
        _repo.GetByIdAsync(Arg.Any<EvacuationRouteId>(), Arg.Any<CancellationToken>()).Returns((EvacuationRoute?)null);

        var command = new ClearRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), Guid.NewGuid(), TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.NotFound");
    }

    [Fact]
    public async Task Handle_WithUnknownObstructionId_ReturnsFailureWithoutPropagatingDomainException()
    {
        var route = CreatedRouteWithObstruction(out _);
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var command = new ClearRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            route.EvacuationRouteId.Value, Guid.NewGuid(), TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.ClearObstructionFailed");
    }

    [Fact]
    public async Task Handle_ClearingAlreadyClearedObstruction_ReturnsFailure()
    {
        var route = CreatedRouteWithObstruction(out var obstructionId);
        route.ClearObstruction(obstructionId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var command = new ClearRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            route.EvacuationRouteId.Value, obstructionId.Value, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("a re-clear of an already-cleared obstruction must be rejected, not silently accepted");
    }

    [Fact]
    public async Task Handle_ShouldNotMutateOtherOpenObstructionsOnTheSameRoute()
    {
        var route = CreatedRouteWithObstruction(out var firstObstructionId);
        var secondObstructionId = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids.RouteObstructionId.New();
        route.RecordObstruction(secondObstructionId, "Second obstruction", PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums.ObstructionSeverity.Minor,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var command = new ClearRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            route.EvacuationRouteId.Value, firstObstructionId.Value, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Obstructed", "the second obstruction remains open");
        route.ObstructionHistory.Single(o => o.RouteObstructionId.Equals(secondObstructionId)).ClearedAtUtc.Should().BeNull();
    }
}
