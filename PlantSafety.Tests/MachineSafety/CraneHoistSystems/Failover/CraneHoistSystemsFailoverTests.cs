// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Failover
//  Layer:          Tests (Failover)
//  File:           CraneHoistSystemsFailoverTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Failover tests verifying that when a write to the primary repository fails
//                  mid-command, the in-memory aggregate the caller retains is not left silently
//                  half-applied for a *subsequent* successful retry against a healthy repository
//                  path -- i.e. re-issuing the same command after a failover produces the same
//                  end state as if the first attempt had never happened. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Failover;

public sealed class CraneHoistSystemsFailoverTests : UnitTestBase
{
    [Fact]
    public async Task RegisterCraneHoistAsset_WhenPrimaryWriteFailsThenRetried_SecondAttemptSucceedsCleanly()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var handler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());

        // Simulate the primary write path failing over (e.g. SQL Server failover event mid-INSERT).
        repo.FailNextCall = new TimeoutException("primary replica failover in progress");

        var command = new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "FAILOVER-1", "Failover Crane", "Hoist", 1500m, TestUserId.Value);

        var firstAttempt = await handler.Handle(command, CancellationToken.None);
        firstAttempt.IsFailure.Should().BeTrue();

        // Caller retries the identical command once the new primary is promoted.
        var secondAttempt = await handler.Handle(command, CancellationToken.None);

        secondAttempt.IsSuccess.Should().BeTrue();
        secondAttempt.Value.Status.Should().Be("Registered");
    }

    [Fact]
    public async Task RecordOverloadEvent_WhenUpdateFailsOverMidCommand_RetriedCommandStillAppliesExactlyOnce()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "FAILOVER-2", "Failover Crane Two", CraneHoistAssetType.MobileCrane.ToString(), 4000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        var overloadHandler = new RecordOverloadEventCommandHandler(repo, NullLogger<RecordOverloadEventCommandHandler>());
        var command = new RecordOverloadEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            craneId, 5000m, TestUserId.Value);

        repo.FailNextCall = new InvalidOperationException("connection reset during failover");
        var failedAttempt = await overloadHandler.Handle(command, CancellationToken.None);
        failedAttempt.IsFailure.Should().BeTrue();

        var retriedAttempt = await overloadHandler.Handle(command, CancellationToken.None);

        retriedAttempt.IsSuccess.Should().BeTrue();
        // Read-side confirms exactly one overload event was durably applied -- the failed attempt
        // never reached the (simulated) write path.
        var byId = await repo.GetCraneHoistSystemsItemByIdAsync(TestTenantId.Value, craneId, CancellationToken.None);
        byId!.OverloadEventCount.Should().Be(1);
    }
}
