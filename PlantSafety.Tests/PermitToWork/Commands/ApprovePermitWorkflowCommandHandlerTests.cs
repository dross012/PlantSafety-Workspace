// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PermitToWork.Commands
//  Layer:          Tests (Unit)
//  File:           ApprovePermitWorkflowCommandHandlerTests.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 PermitToWork concurrency remediation.
//                  ApprovePermitWorkflowCommandHandler must forward the caller's ETag to
//                  IPermitWorkflowRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError -- previously the
//                  handler's blanket catch(Exception) would have turned a stale-ETag conflict
//                  into an ordinary 400 failure instead of the 409 Conflict that
//                  ExceptionHandlingMiddleware now maps ConcurrencyConflictException to.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.PermitToWork.Commands;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.ValueObjects.Ids;
using WorkflowAggregate = PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow;

namespace PlantSafety.Tests.PermitToWork.Commands;

public sealed class ApprovePermitWorkflowCommandHandlerTests : UnitTestBase
{
    private readonly IPermitWorkflowRepository _repo = Substitute.For<IPermitWorkflowRepository>();

    private ApprovePermitWorkflowCommandHandler Sut() => new(_repo, NullLogger<ApprovePermitWorkflowCommandHandler>());

    private static WorkflowAggregate ControlsImplementedWorkflow()
    {
        var agg = WorkflowAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            permitNumber: "PTW-0001",
            permitTypeCode: "HOT_WORK",
            workDescription: "Weld repair on line 3");

        agg.AssessHazards("LOTO + fire watch", riskScore: 5, assessedAtUtc: FixedTimestamp, actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);
        agg.ImplementControls("J. Smith", implementedAtUtc: FixedTimestamp, actor: TestActor, correlationId: TestCorrelationId, causationId: TestCausationId);
        agg.ClearDomainEvents();

        return agg;
    }

    private static ApprovePermitWorkflowCommand ApproveCommandFor(WorkflowAggregate workflow, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            PermitWorkflowId: workflow.PermitWorkflowId.Value,
            ApprovedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var workflow = ControlsImplementedWorkflow();
        _repo.GetByIdAsync(Arg.Any<PermitWorkflowId>(), Arg.Any<CancellationToken>()).Returns(workflow);

        var command = ApproveCommandFor(workflow, "0x0000000000001234");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(workflow, "0x0000000000001234", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var workflow = ControlsImplementedWorkflow();
        _repo.GetByIdAsync(Arg.Any<PermitWorkflowId>(), Arg.Any<CancellationToken>()).Returns(workflow);
        _repo.UpdateAsync(Arg.Any<WorkflowAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("PermitWorkflow was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ApproveCommandFor(workflow, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
