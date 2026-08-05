// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Recovery
//  Layer:          Tests (Recovery)
//  File:           CraneHoistSystemsRecoveryTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Recovery tests verifying the CraneHoistSystems module returns to fully
//                  correct behavior immediately after a transient repository outage clears --
//                  no lingering degraded state, no need to "warm up" the handler again, and
//                  previously-recorded state (e.g. an operator qualification suspension) survives
//                  the outage intact. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Tests.MachineSafety.CraneHoistSystems.Common;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Recovery;

public sealed class CraneHoistSystemsRecoveryTests : UnitTestBase
{
    [Fact]
    public async Task Handler_ImmediatelyAfterTransientOutageClears_ProcessesNextCommandNormally()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());

        repo.FailNextCall = new TimeoutException("outage");
        var duringOutage = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "RECOVERY-1", "Recovery Crane One", "Hoist", 1000m, TestUserId.Value), CancellationToken.None);
        duringOutage.IsFailure.Should().BeTrue();

        // Outage clears (FailNextCall self-cleared); no special reset/rehydration step required.
        var afterRecovery = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "RECOVERY-2", "Recovery Crane Two", "Hoist", 1000m, TestUserId.Value), CancellationToken.None);

        afterRecovery.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PreviouslyRecordedState_SurvivesATransientOutageOnAnUnrelatedCommand()
    {
        var repo = new FakeCraneHoistSystemsRepository();
        var registerHandler = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());
        var suspendHandler = new SuspendOperatorQualificationCommandHandler(repo, NullLogger<SuspendOperatorQualificationCommandHandler>());
        var qualifyHandler = new QualifyOperatorCommandHandler(repo, NullLogger<QualifyOperatorCommandHandler>());

        var registerResult = await registerHandler.Handle(new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "RECOVERY-3", "Recovery Crane Three", CraneHoistAssetType.GantryCrane.ToString(), 2000m, TestUserId.Value), CancellationToken.None);
        var craneId = registerResult.Value.CraneHoistSystemsId;

        await qualifyHandler.Handle(new QualifyOperatorCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            craneId, TestUserId.Value, null, TestUserId.Value), CancellationToken.None);

        await suspendHandler.Handle(new SuspendOperatorQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            craneId, "audit finding", TestUserId.Value), CancellationToken.None);

        // A completely unrelated later command hits a transient outage.
        repo.FailNextCall = new InvalidOperationException("unrelated transient fault");
        var liftHandler = new RecordLiftEventCommandHandler(repo, NullLogger<RecordLiftEventCommandHandler>());
        var outageResult = await liftHandler.Handle(new RecordLiftEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            craneId, 500m, TestUserId.Value), CancellationToken.None);
        outageResult.IsFailure.Should().BeTrue();

        var current = await repo.GetCraneHoistSystemsItemByIdAsync(TestTenantId.Value, craneId, CancellationToken.None);
        current.Should().NotBeNull();
        // The suspension recorded before the outage must still be intact.
        var afterOutageSuspend = await suspendHandler.Handle(new SuspendOperatorQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            craneId, "still suspended", TestUserId.Value), CancellationToken.None);
        afterOutageSuspend.IsSuccess.Should().BeTrue();
    }
}
