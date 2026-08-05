// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCognitiveLoadByWorkerQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCognitiveLoadByWorkerQueryHandler: successful retrieval,
//                   not-found mapping, and unexpected exception mapping. Data governance: this
//                   query returns single-worker sensitive data, so the not-found path must not
//                   otherwise leak whether the worker exists at all in a differently-scoped
//                   period. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Handlers;

public sealed class GetCognitiveLoadByWorkerQueryHandlerTests : UnitTestBase
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repo = Substitute.For<ICognitiveLoadAnalyticsReadRepository>();

    private GetCognitiveLoadByWorkerQueryHandler Sut() => new(_repo, NullLogger<GetCognitiveLoadByWorkerQueryHandler>());

    private static GetCognitiveLoadByWorkerQuery Query(Guid workerId) =>
        new(TestTenantId.Value, workerId, FixedUtcNow.AddDays(-7), FixedUtcNow);

    [Fact]
    public async Task Handle_WhenReadingsExist_ReturnsRollup()
    {
        var workerId = TestUserId.Value;
        var dto = new CognitiveLoadByWorkerDto
        {
            WorkerUserId = workerId, PeriodStartUtc = FixedUtcNow.AddDays(-7), PeriodEndUtc = FixedUtcNow,
            MeanCognitiveLoadScore = 55m, PeakCognitiveLoadScore = 90m, OverloadEventCount = 1, ReadingCount = 5,
        };
        _repo.GetCognitiveLoadByWorkerAsync(TestTenantId.Value, workerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(dto);

        var result = await Sut().Handle(Query(workerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Handle_WhenNoReadingsExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetCognitiveLoadByWorkerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns((CognitiveLoadByWorkerDto?)null);

        var result = await Sut().Handle(Query(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveLoadByWorker.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureWithoutLeakingException()
    {
        _repo.GetCognitiveLoadByWorkerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<CognitiveLoadByWorkerDto?>>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(Query(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveLoadByWorker.RetrievalFailed");
        result.Error.Message.Should().NotContain("boom");
    }
}
