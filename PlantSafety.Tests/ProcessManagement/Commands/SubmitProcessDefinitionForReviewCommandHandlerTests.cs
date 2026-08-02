// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.ProcessManagement.Commands
//  Layer:          Tests (Unit)
//  File:           SubmitProcessDefinitionForReviewCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 ProcessManagement full build-out.
//                  SubmitProcessDefinitionForReviewCommandHandler must forward the caller's ETag
//                  to IProcessDefinitionRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.ProcessManagement.Commands;
using PlantSafety.Domain.ProcessManagement.Repositories;
using PlantSafety.Domain.ProcessManagement.ValueObjects;
using ProcessDefinitionAggregate = PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition;

namespace PlantSafety.Tests.ProcessManagement.Commands;

public sealed class SubmitProcessDefinitionForReviewCommandHandlerTests : UnitTestBase
{
    private readonly IProcessDefinitionRepository _repo = Substitute.For<IProcessDefinitionRepository>();

    private SubmitProcessDefinitionForReviewCommandHandler Sut() => new(_repo, NullLogger<SubmitProcessDefinitionForReviewCommandHandler>());

    private static ProcessDefinitionAggregate DraftProcessDefinition() =>
        ProcessDefinitionAggregate.Create(
            tenantId: TestTenantId.Value,
            siteId: null,
            processName: "Confined Space Entry - Tank Farm B",
            category: "ConfinedSpace",
            ownerId: TestUserId,
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor);

    private static SubmitProcessDefinitionForReviewCommand SubmitCommandFor(ProcessDefinitionAggregate processDefinition, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ProcessDefinitionId: processDefinition.ProcessDefinitionId.Value,
            SubmittedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var processDefinition = DraftProcessDefinition();
        _repo.GetByIdAsync(Arg.Any<ProcessDefinitionId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(processDefinition);

        var command = SubmitCommandFor(processDefinition, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(processDefinition, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var processDefinition = DraftProcessDefinition();
        _repo.GetByIdAsync(Arg.Any<ProcessDefinitionId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(processDefinition);
        _repo.UpdateAsync(Arg.Any<ProcessDefinitionAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("ProcessDefinition was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = SubmitCommandFor(processDefinition, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProcessDefinitionDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<ProcessDefinitionId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProcessDefinitionAggregate?)null);

        var command = new SubmitProcessDefinitionForReviewCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ProcessDefinitionId: Guid.NewGuid(),
            SubmittedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ProcessManagement.ProcessDefinition.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing ProcessDefinition record must map to HTTP 404, not the default 500");
    }
}
