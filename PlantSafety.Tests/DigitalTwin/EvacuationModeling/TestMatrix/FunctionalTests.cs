// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Functional
//  File:           FunctionalTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Functional test type: verifies each of the module's 8 write commands produces
//                  the functionally correct, persisted outcome against a real handler + in-memory
//                  repository, scaled to this 4-aggregate module. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class FunctionalTests : UnitTestBase
{
    [Fact]
    public async Task DefineEvacuationRoute_ThenRetire_PersistsExpectedFinalState()
    {
        var repo = new InMemoryEvacuationRouteRepository();
        var defineHandler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var retireHandler = new RetireEvacuationRouteCommandHandler(repo, NullLogger<RetireEvacuationRouteCommandHandler>());

        var defineResult = await defineHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Corridor A", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 120, TestUserId.Value),
            CancellationToken.None);
        defineResult.IsSuccess.Should().BeTrue();

        var retireResult = await retireHandler.Handle(new RetireEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            defineResult.Value.EvacuationRouteId, TestUserId.Value), CancellationToken.None);

        retireResult.IsSuccess.Should().BeTrue();
        retireResult.Value.Status.Should().Be("Retired");
        var persisted = await repo.GetByIdAsync(EvacuationRouteId.FromGuid(defineResult.Value.EvacuationRouteId), CancellationToken.None);
        persisted!.Status.Should().Be(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums.EvacuationRouteStatus.Retired);
    }

    [Fact]
    public async Task RecordRouteObstruction_ThenClear_PersistsExpectedFinalState()
    {
        var repo = new InMemoryEvacuationRouteRepository();
        var defineHandler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var obstructHandler = new RecordRouteObstructionCommandHandler(repo, NullLogger<RecordRouteObstructionCommandHandler>());
        var clearHandler = new ClearRouteObstructionCommandHandler(repo, NullLogger<ClearRouteObstructionCommandHandler>());

        var defineResult = await defineHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Corridor A", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 120, TestUserId.Value),
            CancellationToken.None);
        var routeId = defineResult.Value.EvacuationRouteId;

        var obstructResult = await obstructHandler.Handle(new RecordRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            routeId, "Blocked door", "Severe", TestUserId.Value), CancellationToken.None);
        obstructResult.IsSuccess.Should().BeTrue();
        obstructResult.Value.Status.Should().Be("Obstructed");

        var persistedAfterObstruct = await repo.GetByIdAsync(EvacuationRouteId.FromGuid(routeId), CancellationToken.None);
        var obstructionId = persistedAfterObstruct!.ObstructionHistory.Single().RouteObstructionId.Value;

        var clearResult = await clearHandler.Handle(new ClearRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            routeId, obstructionId, TestUserId.Value), CancellationToken.None);

        clearResult.IsSuccess.Should().BeTrue();
        clearResult.Value.Status.Should().Be("Active");
    }

    [Fact]
    public async Task DefineEvacuationZone_ThenRecordOccupancySnapshot_PersistsExpectedFinalState()
    {
        var repo = new InMemoryEvacuationZoneRepository();
        var defineHandler = new DefineEvacuationZoneCommandHandler(repo, NullLogger<DefineEvacuationZoneCommandHandler>());
        var occupancyHandler = new RecordOccupancySnapshotCommandHandler(repo, NullLogger<RecordOccupancySnapshotCommandHandler>());

        var defineResult = await defineHandler.Handle(new DefineEvacuationZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Zone A", "v2026.1.0", 200, 150, TestUserId.Value), CancellationToken.None);
        defineResult.IsSuccess.Should().BeTrue();

        var occupancyResult = await occupancyHandler.Handle(new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            defineResult.Value.EvacuationZoneId, 175, "BmsFeed", TestUserId.Value), CancellationToken.None);

        occupancyResult.IsSuccess.Should().BeTrue();
        occupancyResult.Value.Status.Should().Be("OccupancyThresholdBreached");

        var persisted = await repo.GetByIdAsync(EvacuationZoneId.FromGuid(defineResult.Value.EvacuationZoneId), CancellationToken.None);
        persisted!.CurrentOccupancyCount.Should().Be(175);
    }

    [Fact]
    public async Task RunEgressSimulation_ThenLinkToDrill_PersistsExpectedFinalState()
    {
        var routeRepo = new InMemoryEvacuationRouteRepository();
        var simulationRepo = new InMemoryEgressSimulationRunRepository();
        var bottleneckRepo = new InMemoryEvacuationBottleneckRepository();

        var runHandler = new RunEgressSimulationCommandHandler(
            simulationRepo, routeRepo, bottleneckRepo, new EgressSimulationBottleneckAnalysisService(),
            NullLogger<RunEgressSimulationCommandHandler>());
        var linkHandler = new LinkSimulationToDrillCommandHandler(simulationRepo, NullLogger<LinkSimulationToDrillCommandHandler>());

        var runResult = await runHandler.Handle(new RunEgressSimulationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), null, "v2026.1.0", "QuarterlyFullSite", 400, 60, TestUserId.Value), CancellationToken.None);
        runResult.IsSuccess.Should().BeTrue();
        runResult.Value.Status.Should().Be("Completed");

        var drillId = Guid.NewGuid();
        var linkResult = await linkHandler.Handle(new LinkSimulationToDrillCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            runResult.Value.EgressSimulationRunId, drillId, TestUserId.Value), CancellationToken.None);

        linkResult.IsSuccess.Should().BeTrue();
        var persisted = await simulationRepo.GetByIdAsync(
            PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids.EgressSimulationRunId.FromGuid(runResult.Value.EgressSimulationRunId),
            CancellationToken.None);
        persisted!.LinkedDrillId.Should().Be(drillId);
    }
}
