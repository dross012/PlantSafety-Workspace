// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Handlers
//  Layer:          Tests (Unit)
//  File:           GetWorkloadTrendByShiftQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetWorkloadTrendByShiftQueryHandler: successful retrieval
//                   (including empty-list, which is a success rather than not-found for this
//                   list-shaped query), and unexpected exception mapping. Isolated, deterministic,
//                   <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Handlers;

public sealed class GetWorkloadTrendByShiftQueryHandlerTests : UnitTestBase
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repo = Substitute.For<ICognitiveLoadAnalyticsReadRepository>();

    private GetWorkloadTrendByShiftQueryHandler Sut() => new(_repo, NullLogger<GetWorkloadTrendByShiftQueryHandler>());

    private static GetWorkloadTrendByShiftQuery Query() =>
        new(TestTenantId.Value, null, "Day", FixedUtcNow.AddDays(-1), FixedUtcNow);

    [Fact]
    public async Task Handle_WhenSnapshotsExist_ReturnsList()
    {
        var items = new List<WorkloadTrendByShiftDto>
        {
            new() { WorkloadTrendRecordId = Guid.NewGuid(), CognitiveLoadLedgerId = Guid.NewGuid(), ShiftType = "Day", MeanCognitiveLoadScore = 50m },
        };
        _repo.GetWorkloadTrendByShiftAsync(TestTenantId.Value, null, "Day", Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var result = await Sut().Handle(Query(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_WhenNoSnapshotsExist_ReturnsSuccessWithEmptyList()
    {
        _repo.GetWorkloadTrendByShiftAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<WorkloadTrendByShiftDto>());

        var result = await Sut().Handle(Query(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureWithoutLeakingException()
    {
        _repo.GetWorkloadTrendByShiftAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<WorkloadTrendByShiftDto>>>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(Query(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.WorkloadTrendByShift.RetrievalFailed");
        result.Error.Message.Should().NotContain("boom");
    }
}
