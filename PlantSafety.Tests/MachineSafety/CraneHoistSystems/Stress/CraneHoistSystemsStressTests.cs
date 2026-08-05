// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Stress
//  Layer:          Tests (Stress)
//  File:           CraneHoistSystemsStressTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Burst/volume stress tests scaled to the CraneHoistSystems module (a single
//                  flat aggregate covering 6 conceptual entities) -- registers a batch of assets
//                  and drives a high-volume burst of concurrent lift/overload events against them,
//                  asserting no exceptions escape and final in-memory state is exactly consistent
//                  with the number of applied operations (per SR01 graceful-degradation-not-
//                  failure and PR11 burst-handling principles in the Performance & Stress Rules
//                  and Design Manual). Isolated, deterministic, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Stress;

public sealed class CraneHoistSystemsStressTests : UnitTestBase
{
    [Fact]
    public async Task RecordLiftEvent_500SequentialBurstCalls_AllSucceedWithConsistentFinalState()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "STRESS-1", "Stress Crane", CraneHoistAssetType.OverheadBridgeCrane.ToString(), 10000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        var liftHandler = new RecordLiftEventCommandHandler(repo, NullLogger<RecordLiftEventCommandHandler>());
        const int burstSize = 500;

        for (var i = 0; i < burstSize; i++)
        {
            var result = await liftHandler.Handle(new RecordLiftEventCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                craneId, 100m + (i % 50), TestUserId.Value), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        var final = await repo.GetCraneHoistSystemsItemByIdAsync(TestTenantId.Value, craneId, CancellationToken.None);
        final.Should().NotBeNull();
        final!.LastObservedLoadKg.Should().Be(100m + ((burstSize - 1) % 50));
    }

    [Fact]
    public async Task RegisterCraneHoistAsset_100ConcurrentRegistrations_AllSucceedWithUniqueIdentifiers()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var handler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        const int concurrency = 100;

        var tasks = Enumerable.Range(0, concurrency).Select(i => handler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, $"STRESS-BURST-{i}", $"Stress Burst Crane {i}", "Hoist", 1000m, TestUserId.Value), CancellationToken.None));

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(r => r.IsSuccess);
        results.Select(r => r.Value.CraneHoistSystemsId).Distinct().Should().HaveCount(concurrency);
    }

    [Fact]
    public async Task RecordOverloadEvent_1000BurstEvents_OverloadCounterStaysExact()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "STRESS-2", "Stress Crane Two", CraneHoistAssetType.JibCrane.ToString(), 1000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        var overloadHandler = new RecordOverloadEventCommandHandler(repo, NullLogger<RecordOverloadEventCommandHandler>());
        const int burstSize = 1000;

        for (var i = 0; i < burstSize; i++)
        {
            var result = await overloadHandler.Handle(new RecordOverloadEventCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                craneId, 1500m, TestUserId.Value), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        var final = await repo.GetCraneHoistSystemsItemByIdAsync(TestTenantId.Value, craneId, CancellationToken.None);
        final!.OverloadEventCount.Should().Be(burstSize);
    }
}
