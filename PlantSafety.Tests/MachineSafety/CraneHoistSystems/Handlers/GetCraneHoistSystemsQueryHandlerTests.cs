// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Handlers
//  Layer:          Tests (Unit)
//  File:           GetCraneHoistSystemsQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetCraneHoistSystemsQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Handlers;

public sealed class GetCraneHoistSystemsQueryHandlerTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private GetCraneHoistSystemsQueryHandler Sut() => new(_repo, NullLogger<GetCraneHoistSystemsQueryHandler>());

    private static CraneHoistSystemsItemDto ItemFor(Guid id) => new()
    {
        CraneHoistSystemsId = id,
        TenantId = TestTenantId.Value,
        AssetTag = "CRANE-1",
        AssetTitle = "Crane One",
        AssetType = "OverheadBridgeCrane",
        RatedLoadCapacityKg = 5000m,
        SystemStatus = "Registered",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_ReturnsItemsFromRepository()
    {
        var items = new List<CraneHoistSystemsItemDto> { ItemFor(Guid.NewGuid()) };
        _repo.GetCraneHoistSystemsItemsAsync(TestTenantId.Value, null, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var query = new GetCraneHoistSystemsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEquivalentTo(items);
        result.Value.TenantId.Should().Be(TestTenantId.Value);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ReturnsSuccessWithEmptyList()
    {
        _repo.GetCraneHoistSystemsItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<CraneHoistSystemsItemDto>());

        var query = new GetCraneHoistSystemsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureRatherThanPropagating()
    {
        _repo.GetCraneHoistSystemsItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CraneHoistSystemsItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var query = new GetCraneHoistSystemsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-30), FixedUtcNow);
        var result = await Sut().Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.RetrievalFailed");
    }
}
