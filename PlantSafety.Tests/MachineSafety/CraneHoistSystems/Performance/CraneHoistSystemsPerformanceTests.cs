// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Performance
//  Layer:          Tests (Performance)
//  File:           CraneHoistSystemsPerformanceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lightweight latency-budget tests for CraneHoistSystems command/query handlers
//                  against an in-memory repository, pinned to the Command/Query Handler budget
//                  (P50/P95/P99 = 75/200/400ms) from the Performance & Stress Rules and Design
//                  Manual Appendix (quick-reference latency table). In-memory timings are far
//                  below the real (DB-backed) budget by design -- these tests catch gross
//                  in-process regressions (accidental O(n^2) loops, synchronous blocking calls),
//                  not database-tier latency, which is covered by the Testcontainers-backed
//                  integration test suite. Isolated, deterministic, no network.
// ============================================================================
using System.Diagnostics;
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Performance;

public sealed class CraneHoistSystemsPerformanceTests : UnitTestBase
{
    // Command/Query Handler P95 budget per the Performance & Stress Rules and Design Manual
    // Appendix quick-reference latency table. Generously applied here since this is an
    // in-memory, non-DB-backed execution path.
    private static readonly TimeSpan P95Budget = TimeSpan.FromMilliseconds(200);

    [Fact]
    public async Task RegisterCraneHoistAsset_CompletesWithinCommandHandlerP95Budget()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var handler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var command = new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "PERF-1", "Performance Crane", "Hoist", 1000m, TestUserId.Value);

        var stopwatch = Stopwatch.StartNew();
        var result = await handler.Handle(command, CancellationToken.None);
        stopwatch.Stop();

        result.IsSuccess.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(P95Budget);
    }

    [Fact]
    public async Task RecordLiftEvent_CompletesWithinCommandHandlerP95Budget()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "PERF-2", "Performance Crane Two", CraneHoistAssetType.MobileCrane.ToString(), 5000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        var liftHandler = new RecordLiftEventCommandHandler(repo, NullLogger<RecordLiftEventCommandHandler>());
        var command = new RecordLiftEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, craneId, 2000m, TestUserId.Value);

        var stopwatch = Stopwatch.StartNew();
        var result = await liftHandler.Handle(command, CancellationToken.None);
        stopwatch.Stop();

        result.IsSuccess.Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(P95Budget);
    }

    [Fact]
    public async Task GetCraneHoistSystems_Average100CallLatency_StaysWithinBudget()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        for (var i = 0; i < 25; i++)
        {
            await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                null, $"PERF-BULK-{i}", $"Performance Crane {i}", "Hoist", 1000m, TestUserId.Value), CancellationToken.None);
        }

        var queryHandler = new PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.GetCraneHoistSystemsQueryHandler(
            repo, NullLogger<PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers.GetCraneHoistSystemsQueryHandler>());
        var query = new PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries.GetCraneHoistSystemsQuery(
            TestTenantId.Value, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1));

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var result = await queryHandler.Handle(query, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
        stopwatch.Stop();

        var averagePerCall = stopwatch.Elapsed / 100;
        averagePerCall.Should().BeLessThan(P95Budget);
    }
}
