// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Performance / Stress
//  File:           PerformanceStressTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Performance and Stress test types: (Performance) a batch of command handler
//                  invocations against the in-memory repository completes within a generous
//                  wall-clock budget, confirming no accidental O(n^2)/quadratic behavior was
//                  introduced; (Stress) many repeated obstruction record/clear cycles on a single
//                  EvacuationRoute aggregate -- the sub-domain's previously hand-reconciled area --
//                  never corrupt ObstructionCount, ObstructionHistory, or Status. Scaled to this
//                  4-aggregate module (hundreds, not thousands, of iterations). Isolated,
//                  deterministic, no I/O, no network.
// ============================================================================
using System.Diagnostics;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class PerformanceStressTests : UnitTestBase
{
    [Fact]
    public async Task DefineEvacuationRoute_200SequentialInvocations_CompletesWithinBudget()
    {
        var repo = new InMemoryEvacuationRouteRepository();
        var handler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());
        const int iterations = 200;

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var result = await handler.Handle(new DefineEvacuationRouteCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
                $"Route {i}", Guid.NewGuid(), Guid.NewGuid(), $"geo://{i}", "v2026.1.0", 60 + i, TestUserId.Value), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
        stopwatch.Stop();

        repo.AddCallCount.Should().Be(iterations);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5),
            $"{iterations} in-memory command handler invocations must not exhibit quadratic slowdown");
    }

    [Fact]
    public async Task RecordOccupancySnapshot_200SequentialReadings_AllPersistWithoutDegradation()
    {
        var repo = new InMemoryEvacuationZoneRepository();
        var defineHandler = new DefineEvacuationZoneCommandHandler(repo, NullLogger<DefineEvacuationZoneCommandHandler>());
        var zone = await defineHandler.Handle(new DefineEvacuationZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Stress Zone", "v2026.1.0", 1000, 900, TestUserId.Value), CancellationToken.None);

        var occupancyHandler = new RecordOccupancySnapshotCommandHandler(repo, NullLogger<RecordOccupancySnapshotCommandHandler>());
        const int iterations = 200;

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var result = await occupancyHandler.Handle(new RecordOccupancySnapshotCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                zone.Value.EvacuationZoneId, i, "Sensor", TestUserId.Value), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
        stopwatch.Stop();

        var persisted = await repo.GetByIdAsync(
            PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids.EvacuationZoneId.FromGuid(zone.Value.EvacuationZoneId),
            CancellationToken.None);
        persisted!.OccupancyReadings.Should().HaveCount(iterations);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RecordAndClearObstruction_200AlternatingCycles_NeverCorruptsObstructionCountOrHistory()
    {
        // Stress-tests the reconciled RecordObstruction/ClearObstruction interplay under repeated
        // load: 200 alternating obstruct/clear cycles on a single route must leave ObstructionCount
        // exactly equal to the number of RecordObstruction calls and the route Active at the end.
        var repo = new InMemoryEvacuationRouteRepository();
        var defineHandler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var route = await defineHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Stress Route", Guid.NewGuid(), Guid.NewGuid(), "geo://stress", "v2026.1.0", 60, TestUserId.Value), CancellationToken.None);
        var routeId = route.Value.EvacuationRouteId;

        var obstructHandler = new RecordRouteObstructionCommandHandler(repo, NullLogger<RecordRouteObstructionCommandHandler>());
        var clearHandler = new ClearRouteObstructionCommandHandler(repo, NullLogger<ClearRouteObstructionCommandHandler>());
        const int cycles = 200;

        for (var i = 0; i < cycles; i++)
        {
            var obstructResult = await obstructHandler.Handle(new RecordRouteObstructionCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                routeId, $"Obstruction {i}", "Minor", TestUserId.Value), CancellationToken.None);
            obstructResult.IsSuccess.Should().BeTrue();

            var current = await repo.GetByIdAsync(EvacuationRouteId.FromGuid(routeId), CancellationToken.None);
            var openObstructionId = current!.ObstructionHistory.Single(o => o.ClearedAtUtc is null).RouteObstructionId.Value;

            var clearResult = await clearHandler.Handle(new ClearRouteObstructionCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                routeId, openObstructionId, TestUserId.Value), CancellationToken.None);
            clearResult.IsSuccess.Should().BeTrue();
        }

        var final = await repo.GetByIdAsync(EvacuationRouteId.FromGuid(routeId), CancellationToken.None);
        final!.ObstructionCount.Should().Be(cycles);
        final.ObstructionHistory.Should().HaveCount(cycles);
        final.ObstructionHistory.Should().OnlyContain(o => o.ClearedAtUtc != null, "every obstruction must have been cleared by the end of the stress cycle");
        final.Status.Should().Be(EvacuationRouteStatus.Active);
    }
}
