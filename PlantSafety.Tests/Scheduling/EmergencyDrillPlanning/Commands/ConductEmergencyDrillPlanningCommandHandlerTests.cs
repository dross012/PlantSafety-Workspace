// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Scheduling.EmergencyDrillPlanning.Commands
//  Layer:          Tests (Unit)
//  File:           ConductEmergencyDrillPlanningCommandHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Scheduling.EmergencyDrillPlanning full
//                  build-out. ConductEmergencyDrillPlanningCommandHandler must forward the
//                  caller's ETag to IEmergencyDrillPlanningRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;
using PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.ValueObjects.Ids;
using EmergencyDrillPlanningAggregate = PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning;

namespace PlantSafety.Tests.Scheduling.EmergencyDrillPlanning.Commands;

public sealed class ConductEmergencyDrillPlanningCommandHandlerTests : UnitTestBase
{
    private readonly IEmergencyDrillPlanningRepository _repo = Substitute.For<IEmergencyDrillPlanningRepository>();

    private ConductEmergencyDrillPlanningCommandHandler Sut() => new(_repo, NullLogger<ConductEmergencyDrillPlanningCommandHandler>());

    private static EmergencyDrillPlanningAggregate CreatedDrill() =>
        EmergencyDrillPlanningAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            drillType: "FireEvacuation",
            shiftCode: "Day",
            scheduledAtUtc: FixedTimestamp,
            participationCount: 0);

    private static ConductEmergencyDrillPlanningCommand ConductCommandFor(EmergencyDrillPlanningAggregate drill, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EmergencyDrillPlanningId: drill.EmergencyDrillPlanningId.Value,
            ConductedByUserId: TestUserId.Value,
            ParticipationCount: 42)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var drill = CreatedDrill();
        _repo.GetByIdAsync(Arg.Any<EmergencyDrillPlanningId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(drill);

        var command = ConductCommandFor(drill, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(drill, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var drill = CreatedDrill();
        _repo.GetByIdAsync(Arg.Any<EmergencyDrillPlanningId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(drill);
        _repo.UpdateAsync(Arg.Any<EmergencyDrillPlanningAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("EmergencyDrillPlanning was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ConductCommandFor(drill, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
