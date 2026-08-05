// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Handlers
//  Layer:          Tests (Unit)
//  File:           ListLiftEventsByAssetQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListLiftEventsByAssetQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Handlers;

public sealed class ListLiftEventsByAssetQueryHandlerTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private ListLiftEventsByAssetQueryHandler Sut() => new(_repo, NullLogger<ListLiftEventsByAssetQueryHandler>());

    private static CraneHoistSystemsItemDto ItemFor(Guid id) => new()
    {
        CraneHoistSystemsId = id,
        TenantId = TestTenantId.Value,
        AssetTag = "CRANE-1",
        AssetTitle = "Crane One",
        AssetType = "Hoist",
        RatedLoadCapacityKg = 3000m,
        LastObservedLoadKg = 1200m,
        SystemStatus = "InService",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenAssetExists_ReturnsSingleItemList()
    {
        var id = Guid.NewGuid();
        var item = ItemFor(id);
        _repo.GetCraneHoistSystemsItemByIdAsync(TestTenantId.Value, id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new ListLiftEventsByAssetQuery(TestTenantId.Value, id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.LastObservedLoadKg.Should().Be(1200m);
    }

    [Fact]
    public async Task Handle_WhenAssetDoesNotExist_ReturnsEmptyList()
    {
        _repo.GetCraneHoistSystemsItemByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CraneHoistSystemsItemDto?)null);

        var result = await Sut().Handle(new ListLiftEventsByAssetQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureRatherThanPropagating()
    {
        _repo.GetCraneHoistSystemsItemByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<CraneHoistSystemsItemDto?>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListLiftEventsByAssetQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.ListLiftEventsFailed");
    }
}
