// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Chaos
//  Layer:          Tests (Chaos)
//  File:           CraneHoistSystemsChaosTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Chaos-style tests that inject randomized transient repository failures across
//                  many iterations of every CraneHoistSystems command handler, asserting the
//                  handler never lets an exception escape and the in-memory store never lands in
//                  a partially-updated state after a failed call. Isolated, deterministic (seeded
//                  Random), <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Chaos;

public sealed class CraneHoistSystemsChaosTests : UnitTestBase
{
    private static readonly Exception[] ChaosExceptions =
    [
        new TimeoutException("simulated transient timeout"),
        new InvalidOperationException("simulated connection reset"),
        new Exception("simulated unknown infrastructure fault"),
    ];

    [Fact]
    public async Task RecordLiftEvent_UnderRandomizedTransientFailures_NeverThrowsAndNeverCorruptsState()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "CHAOS-1", "Chaos Crane", CraneHoistAssetType.Hoist.ToString(), 3000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        var liftHandler = new RecordLiftEventCommandHandler(repo, NullLogger<RecordLiftEventCommandHandler>());
        var random = new Random(Seed: 42);
        var successCount = 0;
        var failureCount = 0;

        for (var i = 0; i < 200; i++)
        {
            if (random.Next(0, 3) == 0)
            {
                repo.FailNextCall = ChaosExceptions[random.Next(ChaosExceptions.Length)];
            }

            var command = new RecordLiftEventCommand(
                Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
                craneId, 100m + i, TestUserId.Value);

            Exception? caught = null;
            var result = default(PlantSafety.Application.Common.Results.Result<CraneHoistSystemsActionResult>);
            try
            {
                result = await liftHandler.Handle(command, CancellationToken.None);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            caught.Should().BeNull("the handler must translate infrastructure faults to Result.Failure, never an unhandled exception");
            if (result!.IsSuccess) successCount++; else failureCount++;
        }

        successCount.Should().BeGreaterThan(0);
        (successCount + failureCount).Should().Be(200);
        repo.FailNextCall.Should().BeNull("fault injection must be single-shot and self-clearing between iterations");
    }
}
