// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Handlers
//  Layer:          Tests (Unit)
//  File:           ListExpiringOperatorQualificationsQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListExpiringOperatorQualificationsQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Handlers;

public sealed class ListExpiringOperatorQualificationsQueryHandlerTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private ListExpiringOperatorQualificationsQueryHandler Sut() => new(_repo, NullLogger<ListExpiringOperatorQualificationsQueryHandler>());

    [Fact]
    public async Task Handle_ReturnsItemsFromRepository()
    {
        var items = new List<CraneHoistSystemsItemDto>
        {
            new()
            {
                CraneHoistSystemsId = Guid.NewGuid(),
                TenantId = TestTenantId.Value,
                AssetTag = "CRANE-1",
                AssetTitle = "Crane One",
                AssetType = "OverheadBridgeCrane",
                RatedLoadCapacityKg = 5000m,
                SystemStatus = "InService",
                OperatorQualificationExpiresAtUtc = FixedUtcNow.AddDays(10),
                CreatedAtUtc = FixedUtcNow,
                CreatedBy = TestUserId.Value,
            },
        };
        _repo.ListExpiringOperatorQualificationsAsync(TestTenantId.Value, null, 30, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListExpiringOperatorQualificationsQuery(TestTenantId.Value, null, 30), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureRatherThanPropagating()
    {
        _repo.ListExpiringOperatorQualificationsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<CraneHoistSystemsItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListExpiringOperatorQualificationsQuery(TestTenantId.Value, null, 30), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.ListExpiringQualificationsFailed");
    }
}
