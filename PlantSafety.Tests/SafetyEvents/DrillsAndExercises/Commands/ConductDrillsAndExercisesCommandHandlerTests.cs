// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.DrillsAndExercises.Commands
//  Layer:          Tests (Unit)
//  File:           ConductDrillsAndExercisesCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 SafetyEvents.DrillsAndExercises full
//                  build-out. ConductDrillsAndExercisesCommandHandler must forward the caller's
//                  ETag to IDrillsAndExercisesRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Commands;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Interfaces;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.ValueObjects.Ids;
using DrillsAndExercisesAggregate = PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Aggregates.DrillsAndExercises;

namespace PlantSafety.Tests.SafetyEvents.DrillsAndExercises.Commands;

public sealed class ConductDrillsAndExercisesCommandHandlerTests : UnitTestBase
{
    private readonly IDrillsAndExercisesRepository _repo = Substitute.For<IDrillsAndExercisesRepository>();

    private ConductDrillsAndExercisesCommandHandler Sut() => new(_repo, NullLogger<ConductDrillsAndExercisesCommandHandler>());

    private static DrillsAndExercisesAggregate ScheduledDrill() =>
        DrillsAndExercisesAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            drillType: "FireDrill",
            scheduledAtUtc: FixedTimestamp);

    private static ConductDrillsAndExercisesCommand ConductCommandFor(DrillsAndExercisesAggregate drill, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            DrillsAndExercisesId: drill.DrillsAndExercisesId.Value,
            ConductedByUserId: TestUserId.Value,
            ParticipantCount: 42)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var drill = ScheduledDrill();
        _repo.GetByIdAsync(Arg.Any<DrillsAndExercisesId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(drill);

        var command = ConductCommandFor(drill, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(drill, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var drill = ScheduledDrill();
        _repo.GetByIdAsync(Arg.Any<DrillsAndExercisesId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(drill);
        _repo.UpdateAsync(Arg.Any<DrillsAndExercisesAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("DrillsAndExercises was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ConductCommandFor(drill, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
