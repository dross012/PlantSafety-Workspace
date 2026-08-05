// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCraneHoistAssetByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCraneHoistAssetByIdQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Handlers;

public sealed class GetCraneHoistAssetByIdQueryHandlerTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private GetCraneHoistAssetByIdQueryHandler Sut() => new(_repo, NullLogger<GetCraneHoistAssetByIdQueryHandler>());

    private static CraneHoistSystemsItemDto ItemFor(Guid id, Guid tenantId) => new()
    {
        CraneHoistSystemsId = id,
        TenantId = tenantId,
        AssetTag = "CRANE-1",
        AssetTitle = "Crane One",
        AssetType = "OverheadBridgeCrane",
        RatedLoadCapacityKg = 5000m,
        SystemStatus = "Registered",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenAssetExists_ReturnsItem()
    {
        var id = Guid.NewGuid();
        var item = ItemFor(id, TestTenantId.Value);
        _repo.GetCraneHoistSystemsItemByIdAsync(TestTenantId.Value, id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new GetCraneHoistAssetByIdQuery(TestTenantId.Value, id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task Handle_WhenAssetDoesNotExist_ReturnsSuccessWithNullValue()
    {
        _repo.GetCraneHoistSystemsItemByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CraneHoistSystemsItemDto?)null);

        var result = await Sut().Handle(new GetCraneHoistAssetByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureRatherThanPropagating()
    {
        _repo.GetCraneHoistSystemsItemByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<CraneHoistSystemsItemDto?>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetCraneHoistAssetByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.RetrievalFailed");
    }
}
