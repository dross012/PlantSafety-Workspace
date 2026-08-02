// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Operational.PreventiveMaintenance.Commands
//  Layer:          Tests (Unit)
//  File:           StartPreventiveMaintenanceTaskCommandHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Operational.PreventiveMaintenance
//                  PreventiveMaintenanceTask full build-out. StartPreventiveMaintenanceTaskCommandHandler
//                  must forward the caller's ETag to IPreventiveMaintenanceTaskRepository.UpdateAsync,
//                  and must let a ConcurrencyConflictException raised by the repository propagate
//                  unchanged rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Operational.PreventiveMaintenance.Commands;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PreventiveMaintenance.ValueObjects.Ids;
using PreventiveMaintenanceTaskAggregate = PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates.PreventiveMaintenanceTask;

namespace PlantSafety.Tests.Operational.PreventiveMaintenance.Commands;

public sealed class StartPreventiveMaintenanceTaskCommandHandlerTests : UnitTestBase
{
    private readonly IPreventiveMaintenanceTaskRepository _repo = Substitute.For<IPreventiveMaintenanceTaskRepository>();

    private StartPreventiveMaintenanceTaskCommandHandler Sut() => new(_repo, NullLogger<StartPreventiveMaintenanceTaskCommandHandler>());

    private static PreventiveMaintenanceTaskAggregate ScheduledTask() =>
        PreventiveMaintenanceTaskAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            equipmentId: Guid.NewGuid(),
            taskDescription: "Lubricate bearings on Pump 3",
            scheduledAtUtc: FixedTimestamp);

    private static StartPreventiveMaintenanceTaskCommand StartCommandFor(PreventiveMaintenanceTaskAggregate task, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            PreventiveMaintenanceTaskId: task.PreventiveMaintenanceTaskId.Value,
            StartedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var task = ScheduledTask();
        _repo.GetByIdAsync(Arg.Any<PreventiveMaintenanceTaskId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(task);

        var command = StartCommandFor(task, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(task, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var task = ScheduledTask();
        _repo.GetByIdAsync(Arg.Any<PreventiveMaintenanceTaskId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(task);
        _repo.UpdateAsync(Arg.Any<PreventiveMaintenanceTaskAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("PreventiveMaintenanceTask was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = StartCommandFor(task, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
