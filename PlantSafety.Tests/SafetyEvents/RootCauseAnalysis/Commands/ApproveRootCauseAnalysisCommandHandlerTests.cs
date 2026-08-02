// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.RootCauseAnalysis.Commands
//  Layer:          Tests (Unit)
//  File:           ApproveRootCauseAnalysisCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 SafetyEvents.RootCauseAnalysis full
//                  build-out. ApproveRootCauseAnalysisCommandHandler must forward the caller's
//                  ETag to IRootCauseAnalysisRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Commands;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids;
using RootCauseAnalysisAggregate = PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates.RootCauseAnalysis;

namespace PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Commands;

public sealed class ApproveRootCauseAnalysisCommandHandlerTests : UnitTestBase
{
    private readonly IRootCauseAnalysisRepository _repo = Substitute.For<IRootCauseAnalysisRepository>();

    private ApproveRootCauseAnalysisCommandHandler Sut() => new(_repo, NullLogger<ApproveRootCauseAnalysisCommandHandler>());

    private static RootCauseAnalysisAggregate SubmittedRootCauseAnalysis()
    {
        var agg = RootCauseAnalysisAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            relatedEventId: null,
            methodology: "FiveWhys",
            leadAnalyst: TestUserId);

        agg.SubmitForReview(
            submittedBy: TestUserId,
            submittedAtUtc: FixedTimestamp,
            actor: TestActor,
            correlationId: TestCorrelationId,
            causationId: TestCausationId);

        return agg;
    }

    private static ApproveRootCauseAnalysisCommand ApproveCommandFor(RootCauseAnalysisAggregate analysis, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            RootCauseAnalysisId: analysis.RootCauseAnalysisId.Value,
            ApprovedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var analysis = SubmittedRootCauseAnalysis();
        _repo.GetByIdAsync(Arg.Any<RootCauseAnalysisId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(analysis);

        var command = ApproveCommandFor(analysis, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(analysis, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var analysis = SubmittedRootCauseAnalysis();
        _repo.GetByIdAsync(Arg.Any<RootCauseAnalysisId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(analysis);
        _repo.UpdateAsync(Arg.Any<RootCauseAnalysisAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("RootCauseAnalysis was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ApproveCommandFor(analysis, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRootCauseAnalysisDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<RootCauseAnalysisId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RootCauseAnalysisAggregate?)null);

        var command = ApproveCommandFor(SubmittedRootCauseAnalysis(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.RootCauseAnalysis.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing RootCauseAnalysis record must map to HTTP 404, not the default 500");
    }
}
