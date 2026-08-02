// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.WorkerWellBeing.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateWorkerWellBeingCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 WorkerWellBeing full build-out.
//                  ActivateWorkerWellBeingCommandHandler must forward the caller's ETag to
//                  IWorkerWellBeingRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.WorkerWellBeing.Commands;
using PlantSafety.Domain.WorkerWellBeing.Repositories;
using PlantSafety.Domain.WorkerWellBeing.ValueObjects;
using WorkerWellBeingAggregate = PlantSafety.Domain.WorkerWellBeing.Aggregates.WorkerWellBeing;

namespace PlantSafety.Tests.WorkerWellBeing.Commands;

public sealed class ActivateWorkerWellBeingCommandHandlerTests : UnitTestBase
{
    private readonly IWorkerWellBeingRepository _repo = Substitute.For<IWorkerWellBeingRepository>();

    private ActivateWorkerWellBeingCommandHandler Sut() => new(_repo, NullLogger<ActivateWorkerWellBeingCommandHandler>());

    private static WorkerWellBeingAggregate CreatedWorkerWellBeing() =>
        WorkerWellBeingAggregate.Create(
            tenantId: TestTenantId.Value,
            siteId: null,
            workerId: TestUserId,
            wellBeingRiskLevel: "Low",
            checkInType: "MentalHealth",
            wellBeingScore: 7,
            followUpRequired: false,
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor);

    private static ActivateWorkerWellBeingCommand ActivateCommandFor(WorkerWellBeingAggregate workerWellBeing, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            WorkerWellBeingId: workerWellBeing.WorkerWellBeingId.Value,
            ActivatedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var workerWellBeing = CreatedWorkerWellBeing();
        _repo.GetByIdAsync(Arg.Any<WorkerWellBeingId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(workerWellBeing);

        var command = ActivateCommandFor(workerWellBeing, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(workerWellBeing, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var workerWellBeing = CreatedWorkerWellBeing();
        _repo.GetByIdAsync(Arg.Any<WorkerWellBeingId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(workerWellBeing);
        _repo.UpdateAsync(Arg.Any<WorkerWellBeingAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("WorkerWellBeing was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(workerWellBeing, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenWorkerWellBeingDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<WorkerWellBeingId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((WorkerWellBeingAggregate?)null);

        var command = new ActivateWorkerWellBeingCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            WorkerWellBeingId: Guid.NewGuid(),
            ActivatedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("WorkerWellBeing.WorkerWellBeing.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing WorkerWellBeing record must map to HTTP 404, not the default 500");
    }
}
