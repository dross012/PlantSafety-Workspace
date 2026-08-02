// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.ContinuousImprovement.RCA.Commands
//  Layer:          Tests (Unit)
//  File:           StartAnalysisRCACommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 ContinuousImprovement.RCA full build-out.
//                  StartAnalysisRCACommandHandler must forward the caller's ETag to
//                  IRCARepository.UpdateAsync, and must let a ConcurrencyConflictException raised
//                  by the repository propagate unchanged rather than swallowing it into a generic
//                  ApplicationError. Mirrors
//                  PlantSafety.Tests.SafetyEvents.RootCauseAnalysis.Commands
//                  .ApproveRootCauseAnalysisCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.ContinuousImprovement.RCA.Commands;
using PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces;
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;
using RCAAggregate = PlantSafety.Domain.ContinuousImprovement.RCA.Aggregates.RCA;

namespace PlantSafety.Tests.ContinuousImprovement.RCA.Commands;

public sealed class StartAnalysisRCACommandHandlerTests : UnitTestBase
{
    private readonly IRCARepository _repo = Substitute.For<IRCARepository>();

    private StartAnalysisRCACommandHandler Sut() => new(_repo, NullLogger<StartAnalysisRCACommandHandler>());

    private static RCAAggregate InitiatedRCA() =>
        RCAAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            problemStatement: "Unplanned line stoppage on Packaging Line 3",
            methodology: "FiveWhys",
            ownerUserId: TestUserId);

    private static StartAnalysisRCACommand StartCommandFor(RCAAggregate rca, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            RcaId: rca.RcaId.Value,
            StartedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var rca = InitiatedRCA();
        _repo.GetByIdAsync(Arg.Any<RCAId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(rca);

        var command = StartCommandFor(rca, "0x0000000000001234");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("InProgress");
        await _repo.Received(1).UpdateAsync(rca, "0x0000000000001234", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var rca = InitiatedRCA();
        _repo.GetByIdAsync(Arg.Any<RCAId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(rca);
        _repo.UpdateAsync(Arg.Any<RCAAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("RCA was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = StartCommandFor(rca, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRCADoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<RCAId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RCAAggregate?)null);

        var command = StartCommandFor(InitiatedRCA(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ContinuousImprovement.RCA.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing RCA record must map to HTTP 404, not the default 500");
    }
}
