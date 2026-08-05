// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Handlers
//  Layer:          Tests (Unit)
//  File:           ListElevatedCognitiveRiskPredictionsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListElevatedCognitiveRiskPredictionsQueryHandler: successful
//                   retrieval, empty-list success, and unexpected exception mapping. Isolated,
//                   deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Handlers;

public sealed class ListElevatedCognitiveRiskPredictionsQueryHandlerTests : UnitTestBase
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repo = Substitute.For<ICognitiveLoadAnalyticsReadRepository>();

    private ListElevatedCognitiveRiskPredictionsQueryHandler Sut() => new(_repo, NullLogger<ListElevatedCognitiveRiskPredictionsQueryHandler>());

    [Fact]
    public async Task Handle_WhenElevatedPredictionsExist_ReturnsList()
    {
        var items = new List<CognitiveRiskPredictionDto>
        {
            new() { CognitiveRiskPredictionId = Guid.NewGuid(), WorkerUserId = Guid.NewGuid(), SourceLedgerId = Guid.NewGuid(), PredictedRiskLevel = "Elevated", Status = "Generated" },
        };
        _repo.ListElevatedCognitiveRiskPredictionsAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListElevatedCognitiveRiskPredictionsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_WhenNoneAreElevated_ReturnsSuccessWithEmptyList()
    {
        _repo.ListElevatedCognitiveRiskPredictionsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new List<CognitiveRiskPredictionDto>());

        var result = await Sut().Handle(new ListElevatedCognitiveRiskPredictionsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureWithoutLeakingException()
    {
        _repo.ListElevatedCognitiveRiskPredictionsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CognitiveRiskPredictionDto>>>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(new ListElevatedCognitiveRiskPredictionsQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.ListElevatedCognitiveRiskPredictions.RetrievalFailed");
        result.Error.Message.Should().NotContain("boom");
    }
}
