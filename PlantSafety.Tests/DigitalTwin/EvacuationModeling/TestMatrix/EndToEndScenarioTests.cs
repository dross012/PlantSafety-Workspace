// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- End-to-End / Scenario
//  File:           EndToEndScenarioTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    End-to-End and Scenario test types: exercises a realistic cross-aggregate
//                  evacuation-modeling workflow spanning all 4 module aggregates end-to-end through
//                  their real command handlers -- define zone, define route, record occupancy,
//                  record + clear an obstruction, run a simulation that surfaces a bottleneck, and
//                  correlate the run to a drill. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class EndToEndScenarioTests : UnitTestBase
{
    [Fact]
    public async Task Scenario_ZoneAndRouteLifecycleThroughSimulationAndBottleneckDetection_CompletesEndToEndAcrossAllFourAggregates()
    {
        // Scenario: a facility team (1) defines an occupancy-tracked zone, (2) defines an
        // evacuation route out of that zone, (3) records a routine occupancy snapshot,
        // (4) an obstruction is detected and later cleared, (5) a quarterly full-site egress
        // simulation is run and materially exceeds the route's estimate -- automatically raising a
        // bottleneck -- and (6) the simulation run is correlated to a real evacuation drill for
        // after-action review. Every step must succeed and the final aggregate states must be
        // internally consistent across all 4 aggregates.
        var zoneRepo = new InMemoryEvacuationZoneRepository();
        var routeRepo = new InMemoryEvacuationRouteRepository();
        var simulationRepo = new InMemoryEgressSimulationRunRepository();
        var bottleneckRepo = new InMemoryEvacuationBottleneckRepository();

        // Step 1: Define the zone.
        var defineZoneHandler = new DefineEvacuationZoneCommandHandler(zoneRepo, NullLogger<DefineEvacuationZoneCommandHandler>());
        var zoneResult = await defineZoneHandler.Handle(new DefineEvacuationZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Production Floor A", "v2026.1.0", 300, 250, TestUserId.Value), CancellationToken.None);
        zoneResult.IsSuccess.Should().BeTrue();
        var zoneId = zoneResult.Value.EvacuationZoneId;

        // Step 2: Define the route out of that zone.
        var defineRouteHandler = new DefineEvacuationRouteCommandHandler(routeRepo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var routeResult = await defineRouteHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Corridor A -> Assembly Point 1", zoneId, Guid.NewGuid(), "geo://twin/path/1", "v2026.1.0", 300, TestUserId.Value), CancellationToken.None);
        routeResult.IsSuccess.Should().BeTrue();
        var routeId = routeResult.Value.EvacuationRouteId;

        // Step 3: Record a routine occupancy snapshot.
        var occupancyHandler = new RecordOccupancySnapshotCommandHandler(zoneRepo, NullLogger<RecordOccupancySnapshotCommandHandler>());
        var occupancyResult = await occupancyHandler.Handle(new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            zoneId, 120, "Sensor", TestUserId.Value), CancellationToken.None);
        occupancyResult.IsSuccess.Should().BeTrue();

        // Step 4: An obstruction is detected, then cleared.
        var obstructHandler = new RecordRouteObstructionCommandHandler(routeRepo, NullLogger<RecordRouteObstructionCommandHandler>());
        var obstructResult = await obstructHandler.Handle(new RecordRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            routeId, "Fire door fault reported via BMS feed", "Moderate", TestUserId.Value), CancellationToken.None);
        obstructResult.IsSuccess.Should().BeTrue();

        var routeAfterObstruction = await routeRepo.GetByIdAsync(EvacuationRouteId.FromGuid(routeId), CancellationToken.None);
        var obstructionId = routeAfterObstruction!.ObstructionHistory.Single().RouteObstructionId.Value;

        var clearHandler = new ClearRouteObstructionCommandHandler(routeRepo, NullLogger<ClearRouteObstructionCommandHandler>());
        var clearResult = await clearHandler.Handle(new ClearRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            routeId, obstructionId, TestUserId.Value), CancellationToken.None);
        clearResult.IsSuccess.Should().BeTrue();
        clearResult.Value.Status.Should().Be("Active");

        // Step 5: Run a quarterly full-site simulation that materially exceeds the route's estimate.
        var runHandler = new RunEgressSimulationCommandHandler(
            simulationRepo, routeRepo, bottleneckRepo, new EgressSimulationBottleneckAnalysisService(),
            NullLogger<RunEgressSimulationCommandHandler>());
        var runResult = await runHandler.Handle(new RunEgressSimulationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            zoneId, routeId, "v2026.1.0", "QuarterlyFullSite", 550, 140, TestUserId.Value), CancellationToken.None);
        runResult.IsSuccess.Should().BeTrue();

        // Step 6: Correlate the run to a real evacuation drill for after-action review.
        var drillId = Guid.NewGuid();
        var linkHandler = new LinkSimulationToDrillCommandHandler(simulationRepo, NullLogger<LinkSimulationToDrillCommandHandler>());
        var linkResult = await linkHandler.Handle(new LinkSimulationToDrillCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            runResult.Value.EgressSimulationRunId, drillId, TestUserId.Value), CancellationToken.None);
        linkResult.IsSuccess.Should().BeTrue();

        // Final-state assertions across all four aggregates.
        var finalZone = await zoneRepo.GetByIdAsync(
            PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids.EvacuationZoneId.FromGuid(zoneId), CancellationToken.None);
        finalZone!.CurrentOccupancyCount.Should().Be(120);

        var finalRoute = await routeRepo.GetByIdAsync(EvacuationRouteId.FromGuid(routeId), CancellationToken.None);
        finalRoute!.Status.Should().Be(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums.EvacuationRouteStatus.Active);
        finalRoute.ObstructionCount.Should().Be(1);

        var finalRun = await simulationRepo.GetByIdAsync(
            PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids.EgressSimulationRunId.FromGuid(runResult.Value.EgressSimulationRunId),
            CancellationToken.None);
        finalRun!.LinkedDrillId.Should().Be(drillId);

        // The 550s simulated time against a 300s route estimate is a 250s delay -- comfortably over
        // the 60s Bottleneck Analysis threshold, so a bottleneck must have been raised automatically.
        var openBottleneck = await bottleneckRepo.GetOpenForRouteAsync(EvacuationRouteId.FromGuid(routeId), CancellationToken.None);
        openBottleneck.Should().NotBeNull("a 250s delay must be automatically flagged by the Bottleneck Analysis subdomain");
        openBottleneck!.DelaySeconds.Should().Be(250);
    }
}
