// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.PredictiveAnalytics.Commands
//  Layer:          Tests (Unit)
//  File:           ReviewPredictiveAnalyticsCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Analytics.PredictiveAnalytics full build-out.
//                  ReviewPredictiveAnalyticsCommandHandler must forward the caller's ETag to
//                  IPredictiveAnalyticsRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.ValueObjects.Ids;
using PredictiveAnalyticsAggregate = PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics;

namespace PlantSafety.Tests.Analytics.PredictiveAnalytics.Commands;

public sealed class ReviewPredictiveAnalyticsCommandHandlerTests : UnitTestBase
{
    private readonly IPredictiveAnalyticsRepository _repo = Substitute.For<IPredictiveAnalyticsRepository>();

    private ReviewPredictiveAnalyticsCommandHandler Sut() => new(_repo, NullLogger<ReviewPredictiveAnalyticsCommandHandler>());

    private static PredictiveAnalyticsAggregate CreatedPrediction() =>
        PredictiveAnalyticsAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            modelName: "IncidentRiskModel-v3",
            predictionType: "IncidentRisk",
            predictedRiskScore: 62.0m,
            confidenceLevel: 88.0m,
            predictionHorizonDays: 30);

    private static ReviewPredictiveAnalyticsCommand ReviewCommandFor(PredictiveAnalyticsAggregate prediction, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            PredictiveAnalyticsId: prediction.PredictiveAnalyticsId.Value,
            ReviewedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var prediction = CreatedPrediction();
        _repo.GetByIdAsync(Arg.Any<PredictiveAnalyticsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(prediction);

        var command = ReviewCommandFor(prediction, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(prediction, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var prediction = CreatedPrediction();
        _repo.GetByIdAsync(Arg.Any<PredictiveAnalyticsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(prediction);
        _repo.UpdateAsync(Arg.Any<PredictiveAnalyticsAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("PredictiveAnalytics was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ReviewCommandFor(prediction, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
