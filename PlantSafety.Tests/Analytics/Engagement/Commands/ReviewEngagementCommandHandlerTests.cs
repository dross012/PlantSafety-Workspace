// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Engagement.Commands
//  Layer:          Tests (Unit)
//  File:           ReviewEngagementCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Analytics.Engagement full build-out.
//                  ReviewEngagementCommandHandler must forward the caller's ETag to
//                  IEngagementRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.Engagement.Commands;
using PlantSafety.Domain.Analytics.Engagement.Interfaces;
using PlantSafety.Domain.Analytics.Engagement.ValueObjects.Ids;
using EngagementAggregate = PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement;

namespace PlantSafety.Tests.Analytics.Engagement.Commands;

public sealed class ReviewEngagementCommandHandlerTests : UnitTestBase
{
    private readonly IEngagementRepository _repo = Substitute.For<IEngagementRepository>();

    private ReviewEngagementCommandHandler Sut() => new(_repo, NullLogger<ReviewEngagementCommandHandler>());

    private static EngagementAggregate CreatedEngagement() =>
        EngagementAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            workerId: TestUserId,
            engagementScore: 72.5m,
            surveyPeriod: "2026-Q3",
            participationChannel: "MobileApp",
            responseCount: 3);

    private static ReviewEngagementCommand ReviewCommandFor(EngagementAggregate engagement, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EngagementId: engagement.EngagementId.Value,
            ReviewedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var engagement = CreatedEngagement();
        _repo.GetByIdAsync(Arg.Any<EngagementId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(engagement);

        var command = ReviewCommandFor(engagement, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(engagement, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var engagement = CreatedEngagement();
        _repo.GetByIdAsync(Arg.Any<EngagementId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(engagement);
        _repo.UpdateAsync(Arg.Any<EngagementAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Engagement was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ReviewCommandFor(engagement, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
