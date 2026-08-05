// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           RetireEvacuationRouteCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RetireEvacuationRouteCommandHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class RetireEvacuationRouteCommandHandlerTests : UnitTestBase
{
    private readonly IEvacuationRouteRepository _repo = Substitute.For<IEvacuationRouteRepository>();

    private RetireEvacuationRouteCommandHandler Sut() => new(_repo, NullLogger<RetireEvacuationRouteCommandHandler>());

    private static EvacuationRoute CreatedRoute() => EvacuationRoute.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0");

    [Fact]
    public async Task Handle_WithExistingRoute_ReturnsSuccessAndRetiresAggregate()
    {
        var route = CreatedRoute();
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var command = new RetireEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            route.EvacuationRouteId.Value, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Retired");
        await _repo.Received(1).UpdateAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRouteNotFound_ReturnsFailureWithNotFoundCode()
    {
        _repo.GetByIdAsync(Arg.Any<EvacuationRouteId>(), Arg.Any<CancellationToken>()).Returns((EvacuationRoute?)null);

        var command = new RetireEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.NotFound");
    }

    [Fact]
    public async Task Handle_WhenAlreadyRetired_ReturnsFailureWithoutPropagatingDomainException()
    {
        var route = CreatedRoute();
        route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var command = new RetireEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            route.EvacuationRouteId.Value, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.RetireFailed");
    }
}
