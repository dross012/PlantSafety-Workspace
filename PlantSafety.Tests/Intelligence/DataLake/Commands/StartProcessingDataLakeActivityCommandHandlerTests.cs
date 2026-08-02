// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.DataLake.Commands
//  Layer:          Tests (Unit)
//  File:           StartProcessingDataLakeActivityCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Intelligence.DataLake full build-out.
//                  StartProcessingDataLakeActivityCommandHandler must forward the caller's ETag to
//                  IDataLakeActivityRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.ContinuousImprovement.RCA.Commands
//                  .StartAnalysisRCACommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Intelligence.DataLake.Interfaces;
using PlantSafety.Domain.Intelligence.DataLake.ValueObjects.Ids;
using DataLakeActivityAggregate = PlantSafety.Domain.Intelligence.DataLake.Aggregates.DataLakeActivity;

namespace PlantSafety.Tests.Intelligence.DataLake.Commands;

public sealed class StartProcessingDataLakeActivityCommandHandlerTests : UnitTestBase
{
    private readonly IDataLakeActivityRepository _repo = Substitute.For<IDataLakeActivityRepository>();

    private StartProcessingDataLakeActivityCommandHandler Sut() => new(_repo, NullLogger<StartProcessingDataLakeActivityCommandHandler>());

    private static DataLakeActivityAggregate IngestedActivity() =>
        DataLakeActivityAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            datasetName: "WorkerFatigueTelemetry",
            zone: "Raw",
            recordCount: 10_000,
            processedBy: TestUserId);

    private static StartProcessingDataLakeActivityCommand StartCommandFor(DataLakeActivityAggregate activity, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            DataLakeActivityId: activity.DataLakeActivityId.Value,
            StartedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var activity = IngestedActivity();
        _repo.GetByIdAsync(Arg.Any<DataLakeActivityId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activity);

        var command = StartCommandFor(activity, "0x0000000000001234");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Processing");
        await _repo.Received(1).UpdateAsync(activity, "0x0000000000001234", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var activity = IngestedActivity();
        _repo.GetByIdAsync(Arg.Any<DataLakeActivityId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(activity);
        _repo.UpdateAsync(Arg.Any<DataLakeActivityAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("DataLakeActivity was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = StartCommandFor(activity, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenDataLakeActivityDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<DataLakeActivityId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DataLakeActivityAggregate?)null);

        var command = StartCommandFor(IngestedActivity(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.DataLake.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing DataLakeActivity record must map to HTTP 404, not the default 500");
    }
}
