// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.AITraining.Commands
//  Layer:          Tests (Unit)
//  File:           StartAITrainingJobCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Intelligence.AITraining full build-out.
//                  StartAITrainingJobCommandHandler must forward the caller's ETag to
//                  IAITrainingJobRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.ContinuousImprovement.RCA.Commands
//                  .StartAnalysisRCACommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.AITraining.Commands;
using PlantSafety.Domain.Intelligence.AITraining.Interfaces;
using PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;
using AITrainingJobAggregate = PlantSafety.Domain.Intelligence.AITraining.Aggregates.AITrainingJob;

namespace PlantSafety.Tests.Intelligence.AITraining.Commands;

public sealed class StartAITrainingJobCommandHandlerTests : UnitTestBase
{
    private readonly IAITrainingJobRepository _repo = Substitute.For<IAITrainingJobRepository>();

    private StartAITrainingJobCommandHandler Sut() => new(_repo, NullLogger<StartAITrainingJobCommandHandler>());

    private static AITrainingJobAggregate QueuedJob() =>
        AITrainingJobAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            modelName: "FatigueRiskClassifier",
            modelVersion: "1.0.0",
            datasetReference: "s3://plantsafety-ml/datasets/fatigue-v1",
            initiatedBy: TestUserId);

    private static StartAITrainingJobCommand StartCommandFor(AITrainingJobAggregate job, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            AITrainingJobId: job.AITrainingJobId.Value,
            StartedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var job = QueuedJob();
        _repo.GetByIdAsync(Arg.Any<AITrainingJobId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(job);

        var command = StartCommandFor(job, "0x0000000000001234");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Running");
        await _repo.Received(1).UpdateAsync(job, "0x0000000000001234", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var job = QueuedJob();
        _repo.GetByIdAsync(Arg.Any<AITrainingJobId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(job);
        _repo.UpdateAsync(Arg.Any<AITrainingJobAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("AITrainingJob was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = StartCommandFor(job, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAITrainingJobDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<AITrainingJobId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AITrainingJobAggregate?)null);

        var command = StartCommandFor(QueuedJob(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.AITraining.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AITrainingJob record must map to HTTP 404, not the default 500");
    }
}
