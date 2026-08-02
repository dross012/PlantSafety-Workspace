// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.ScoreCards.Commands
//  Layer:          Tests (Unit)
//  File:           PublishScoreCardsCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Analytics.ScoreCards full build-out.
//                  PublishScoreCardsCommandHandler must forward the caller's ETag to
//                  IScoreCardsRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.ScoreCards.Commands;
using PlantSafety.Domain.Analytics.ScoreCards.Interfaces;
using PlantSafety.Domain.Analytics.ScoreCards.ValueObjects.Ids;
using ScoreCardsAggregate = PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards;

namespace PlantSafety.Tests.Analytics.ScoreCards.Commands;

public sealed class PublishScoreCardsCommandHandlerTests : UnitTestBase
{
    private readonly IScoreCardsRepository _repo = Substitute.For<IScoreCardsRepository>();

    private PublishScoreCardsCommandHandler Sut() => new(_repo, NullLogger<PublishScoreCardsCommandHandler>());

    private static ScoreCardsAggregate CreatedScoreCards() =>
        ScoreCardsAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            scoreCardName: "Q3 Safety Scorecard",
            reportingPeriod: "2026-Q3",
            kpiCategory: "Safety",
            performanceIndex: 87.5m);

    private static PublishScoreCardsCommand PublishCommandFor(ScoreCardsAggregate scoreCards, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ScoreCardsId: scoreCards.ScoreCardsId.Value,
            PublishedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var scoreCards = CreatedScoreCards();
        _repo.GetByIdAsync(Arg.Any<ScoreCardsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(scoreCards);

        var command = PublishCommandFor(scoreCards, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(scoreCards, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var scoreCards = CreatedScoreCards();
        _repo.GetByIdAsync(Arg.Any<ScoreCardsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(scoreCards);
        _repo.UpdateAsync(Arg.Any<ScoreCardsAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("ScoreCards was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = PublishCommandFor(scoreCards, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
