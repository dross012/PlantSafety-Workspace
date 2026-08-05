// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           RecordRouteObstructionCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordRouteObstructionCommandHandler. Extra-careful regression
//                  coverage around the previously hand-reconciled obstruction-recording flow.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class RecordRouteObstructionCommandHandlerTests : UnitTestBase
{
    private readonly IEvacuationRouteRepository _repo = Substitute.For<IEvacuationRouteRepository>();

    private RecordRouteObstructionCommandHandler Sut() => new(_repo, NullLogger<RecordRouteObstructionCommandHandler>());

    private static EvacuationRoute CreatedRoute() => EvacuationRoute.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0");

    private static RecordRouteObstructionCommand CommandFor(EvacuationRoute route, string severity = "Severe") => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        route.EvacuationRouteId.Value, "Debris blocking corridor", severity, TestUserId.Value);

    [Fact]
    public async Task Handle_WithExistingRoute_ReturnsSuccessAndObstructsAggregate()
    {
        var route = CreatedRoute();
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var result = await Sut().Handle(CommandFor(route), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Obstructed");
        route.ObstructionHistory.Should().ContainSingle(o => o.Description == "Debris blocking corridor");
        await _repo.Received(1).UpdateAsync(route, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownSeverityString_DefaultsToModerate()
    {
        var route = CreatedRoute();
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        await Sut().Handle(CommandFor(route, severity: "NotARealSeverity"), CancellationToken.None);

        route.ObstructionHistory.Single().Severity.Should()
            .Be(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums.ObstructionSeverity.Moderate);
    }

    [Fact]
    public async Task Handle_WhenRouteNotFound_ReturnsFailureWithNotFoundCode()
    {
        _repo.GetByIdAsync(Arg.Any<EvacuationRouteId>(), Arg.Any<CancellationToken>()).Returns((EvacuationRoute?)null);

        var result = await Sut().Handle(new RecordRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "Obstruction", "Minor", TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.NotFound");
    }

    [Fact]
    public async Task Handle_OnRetiredRoute_ReturnsFailureWithoutPropagatingDomainException()
    {
        var route = CreatedRoute();
        route.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var result = await Sut().Handle(CommandFor(route), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.RecordObstructionFailed");
    }

    [Fact]
    public async Task Handle_CalledTwice_ShouldAccumulateObstructionHistory()
    {
        var route = CreatedRoute();
        _repo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        await Sut().Handle(CommandFor(route), CancellationToken.None);
        await Sut().Handle(CommandFor(route), CancellationToken.None);

        route.ObstructionHistory.Should().HaveCount(2);
        route.ObstructionCount.Should().Be(2);
    }
}
