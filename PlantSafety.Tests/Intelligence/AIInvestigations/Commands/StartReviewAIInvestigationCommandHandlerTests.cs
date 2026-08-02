// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.AIInvestigations.Commands
//  Layer:          Tests (Unit)
//  File:           StartReviewAIInvestigationCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Intelligence.AIInvestigations full build-out.
//                  StartReviewAIInvestigationCommandHandler must forward the caller's ETag to
//                  IAIInvestigationRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Commands
//                  .ApproveRootCauseAnalysisCommandHandlerTests. Isolated, deterministic, <50ms
//                  per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.AIInvestigations.Commands;
using PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces;
using PlantSafety.Domain.Intelligence.AIInvestigations.ValueObjects.Ids;
using AIInvestigationAggregate = PlantSafety.Domain.Intelligence.AIInvestigations.Aggregates.AIInvestigation;

namespace PlantSafety.Tests.Intelligence.AIInvestigations.Commands;

public sealed class StartReviewAIInvestigationCommandHandlerTests : UnitTestBase
{
    private readonly IAIInvestigationRepository _repo = Substitute.For<IAIInvestigationRepository>();

    private StartReviewAIInvestigationCommandHandler Sut() => new(_repo, NullLogger<StartReviewAIInvestigationCommandHandler>());

    private static AIInvestigationAggregate OpenedInvestigation() =>
        AIInvestigationAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            incidentReference: "INC-2026-0001",
            aiConfidenceScore: 0.87,
            summary: "Preliminary AI summary",
            assignedTo: TestUserId);

    private static StartReviewAIInvestigationCommand StartReviewCommandFor(AIInvestigationAggregate investigation, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            AIInvestigationId: investigation.AIInvestigationId.Value,
            StartedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var investigation = OpenedInvestigation();
        _repo.GetByIdAsync(Arg.Any<AIInvestigationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(investigation);

        var command = StartReviewCommandFor(investigation, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(investigation, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var investigation = OpenedInvestigation();
        _repo.GetByIdAsync(Arg.Any<AIInvestigationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(investigation);
        _repo.UpdateAsync(Arg.Any<AIInvestigationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("AIInvestigation was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = StartReviewCommandFor(investigation, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAIInvestigationDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<AIInvestigationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AIInvestigationAggregate?)null);

        var command = StartReviewCommandFor(OpenedInvestigation(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.AIInvestigation.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AIInvestigation record must map to HTTP 404, not the default 500");
    }
}
