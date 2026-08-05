// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Queries
//  Layer:          Tests (Unit)
//  File:           ListContainersByAccumulationAreaQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Query handler tests for ListContainersByAccumulationAreaQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Handlers;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Models;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Queries;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Repositories;

namespace PlantSafety.Tests.HazardousWasteManagement.Queries;

public sealed class ListContainersByAccumulationAreaQueryHandlerTests : UnitTestBase
{
    private readonly IWasteContainerRepository _repo = Substitute.For<IWasteContainerRepository>();

    private ListContainersByAccumulationAreaQueryHandler Sut() => new(_repo, NullLogger<ListContainersByAccumulationAreaQueryHandler>());

    private static WasteContainerItemDto SampleItem(Guid tenantId, Guid accumulationAreaId) => new()
    {
        WasteContainerId = Guid.NewGuid(),
        TenantId = tenantId,
        SiteId = null,
        AccumulationAreaId = accumulationAreaId,
        WasteStreamId = Guid.NewGuid(),
        ContainerLabel = "DRUM-001",
        CapacityGallons = 55m,
        RegulatoryTimeLimitDays = 90,
        OpenedAtUtc = FixedUtcNow,
        ClosedAtUtc = null,
        Status = "Open",
    };

    [Fact]
    public async Task Handle_WithMatchingContainers_ReturnsItems()
    {
        var areaId = Guid.NewGuid();
        var items = new List<WasteContainerItemDto> { SampleItem(TestTenantId.Value, areaId), SampleItem(TestTenantId.Value, areaId) };
        _repo.ListContainersByAccumulationAreaAsync(TestTenantId.Value, areaId, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListContainersByAccumulationAreaQuery(TestTenantId.Value, areaId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMatches_ReturnsEmptyList()
    {
        _repo.ListContainersByAccumulationAreaAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<WasteContainerItemDto>());

        var result = await Sut().Handle(new ListContainersByAccumulationAreaQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.ListContainersByAccumulationAreaAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<WasteContainerItemDto>>>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(new ListContainersByAccumulationAreaQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.WasteContainer.ListFailed");
    }
}
