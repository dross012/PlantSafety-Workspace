// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Queries
//  Layer:          Tests (Unit)
//  File:           ListAccumulationAreasNearingTimeLimitQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Query handler tests for ListAccumulationAreasNearingTimeLimitQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Handlers;
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Models;
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Queries;
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Repositories;

namespace PlantSafety.Tests.HazardousWasteManagement.Queries;

public sealed class ListAccumulationAreasNearingTimeLimitQueryHandlerTests : UnitTestBase
{
    private readonly IAccumulationAreaRepository _repo = Substitute.For<IAccumulationAreaRepository>();

    private ListAccumulationAreasNearingTimeLimitQueryHandler Sut() => new(_repo, NullLogger<ListAccumulationAreasNearingTimeLimitQueryHandler>());

    private static AccumulationAreaItemDto SampleItem(Guid tenantId) => new()
    {
        AccumulationAreaId = Guid.NewGuid(),
        TenantId = tenantId,
        SiteId = null,
        AreaName = "Building 3 Satellite Area",
        LocationDescription = "Adjacent to Loading Dock B",
        TimeLimitDays = 90,
        Status = "TimeLimitWarning",
        EarliestContainerDeadlineUtc = FixedUtcNow.AddDays(3),
    };

    [Fact]
    public async Task Handle_WithMatchingAreas_ReturnsItems()
    {
        var items = new List<AccumulationAreaItemDto> { SampleItem(TestTenantId.Value) };
        _repo.ListAccumulationAreasNearingTimeLimitAsync(TestTenantId.Value, null, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListAccumulationAreasNearingTimeLimitQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ScopedToSite_PassesSiteIdThroughToRepository()
    {
        var siteId = Guid.NewGuid();
        _repo.ListAccumulationAreasNearingTimeLimitAsync(TestTenantId.Value, siteId, Arg.Any<CancellationToken>())
            .Returns(new List<AccumulationAreaItemDto>());

        await Sut().Handle(new ListAccumulationAreasNearingTimeLimitQuery(TestTenantId.Value, siteId), CancellationToken.None);

        await _repo.Received(1).ListAccumulationAreasNearingTimeLimitAsync(TestTenantId.Value, siteId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.ListAccumulationAreasNearingTimeLimitAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<AccumulationAreaItemDto>>>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(new ListAccumulationAreasNearingTimeLimitQuery(TestTenantId.Value, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.AccumulationArea.ListNearingTimeLimitFailed");
    }
}
