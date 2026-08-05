// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           RunEgressSimulationCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RunEgressSimulationCommandHandler, including the automatic
//                  Bottleneck Analysis subdomain integration (EgressSimulationBottleneckAnalysisService)
//                  exercised when a RouteId is supplied. Isolated, deterministic, <50ms per test.
//                  No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class RunEgressSimulationCommandHandlerTests : UnitTestBase
{
    private readonly IEgressSimulationRunRepository _simulationRepo = Substitute.For<IEgressSimulationRunRepository>();
    private readonly IEvacuationRouteRepository _routeRepo = Substitute.For<IEvacuationRouteRepository>();
    private readonly IEvacuationBottleneckRepository _bottleneckRepo = Substitute.For<IEvacuationBottleneckRepository>();

    private RunEgressSimulationCommandHandler Sut() => new(
        _simulationRepo, _routeRepo, _bottleneckRepo, new EgressSimulationBottleneckAnalysisService(),
        NullLogger<RunEgressSimulationCommandHandler>());

    private static RunEgressSimulationCommand CommandFor(Guid? routeId, int simulatedSeconds) => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        Guid.NewGuid(), routeId, "v2026.1.0", "Manual", simulatedSeconds, PredictedPeakOccupancy: 40,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithoutRouteId_CompletesSimulationAndSkipsBottleneckAnalysis()
    {
        var result = await Sut().Handle(CommandFor(routeId: null, simulatedSeconds: 300), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Completed");
        result.Value.SimulatedEvacuationTimeSeconds.Should().Be(300);
        await _routeRepo.DidNotReceive().GetByIdAsync(Arg.Any<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids.EvacuationRouteId>(), Arg.Any<CancellationToken>());
        await _bottleneckRepo.DidNotReceive().AddAsync(Arg.Any<EvacuationBottleneck>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithRouteIdAndDelayBelowThreshold_DoesNotRaiseBottleneck()
    {
        var route = EvacuationRoute.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0",
            estimatedEgressTimeSeconds: 300);
        _routeRepo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        // Delay of 30s is below EgressSimulationBottleneckAnalysisService.BottleneckDelayThresholdSeconds (60s).
        await Sut().Handle(CommandFor(route.EvacuationRouteId.Value, simulatedSeconds: 330), CancellationToken.None);

        await _bottleneckRepo.DidNotReceive().AddAsync(Arg.Any<EvacuationBottleneck>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithRouteIdAndDelayAboveThreshold_RaisesNewBottleneck()
    {
        var route = EvacuationRoute.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0",
            estimatedEgressTimeSeconds: 300);
        _routeRepo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);
        _bottleneckRepo.GetOpenForRouteAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns((EvacuationBottleneck?)null);

        // Delay of 200s exceeds the 60s threshold.
        var result = await Sut().Handle(CommandFor(route.EvacuationRouteId.Value, simulatedSeconds: 500), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _bottleneckRepo.Received(1).AddAsync(
            Arg.Is<EvacuationBottleneck>(b => b.RouteId.Equals(route.EvacuationRouteId) && b.DelaySeconds == 200), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithRouteIdAndExistingOpenBottleneck_RecordsRecurrenceInsteadOfCreating()
    {
        var route = EvacuationRoute.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0",
            estimatedEgressTimeSeconds: 300);
        _routeRepo.GetByIdAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(route);

        var existingBottleneck = EvacuationBottleneck.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, routeId: route.EvacuationRouteId,
            simulationRunId: PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids.EgressSimulationRunId.New(),
            bottleneckLocation: "Corridor", delaySeconds: 90);
        _bottleneckRepo.GetOpenForRouteAsync(route.EvacuationRouteId, Arg.Any<CancellationToken>()).Returns(existingBottleneck);

        await Sut().Handle(CommandFor(route.EvacuationRouteId.Value, simulatedSeconds: 500), CancellationToken.None);

        await _bottleneckRepo.DidNotReceive().AddAsync(Arg.Any<EvacuationBottleneck>(), Arg.Any<CancellationToken>());
        await _bottleneckRepo.Received(1).UpdateAsync(existingBottleneck, Arg.Any<CancellationToken>());
        existingBottleneck.RecurrenceCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenRouteIdDoesNotResolve_StillCompletesSimulationSuccessfully()
    {
        _routeRepo.GetByIdAsync(Arg.Any<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids.EvacuationRouteId>(), Arg.Any<CancellationToken>())
            .Returns((EvacuationRoute?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid(), simulatedSeconds: 500), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _bottleneckRepo.DidNotReceive().AddAsync(Arg.Any<EvacuationBottleneck>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNegativeSimulatedTime_ReturnsFailure()
    {
        var result = await Sut().Handle(CommandFor(routeId: null, simulatedSeconds: -1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EgressSimulationRun.RunFailed");
    }
}
