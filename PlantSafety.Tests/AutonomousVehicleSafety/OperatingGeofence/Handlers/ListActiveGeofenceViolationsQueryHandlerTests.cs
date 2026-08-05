// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.OperatingGeofence.Handlers
//  Layer:          Tests (Unit)
//  File:           ListActiveGeofenceViolationsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListActiveGeofenceViolationsQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Handlers;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Models;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Repositories;

namespace PlantSafety.Tests.AutonomousVehicleSafety.OperatingGeofence.Handlers;

public sealed class ListActiveGeofenceViolationsQueryHandlerTests : UnitTestBase
{
    private readonly IOperatingGeofenceRepository _repo = Substitute.For<IOperatingGeofenceRepository>();

    private ListActiveGeofenceViolationsQueryHandler Sut() => new(_repo, NullLogger<ListActiveGeofenceViolationsQueryHandler>());

    private static OperatingGeofenceItemDto ItemFor(Guid id) => new()
    {
        OperatingGeofenceId = id,
        TenantId = TestTenantId.Value,
        GeofenceName = "North Yard Exclusion",
        ZoneType = "ExclusionZone",
        ViolationCount = 3,
        Status = "Active",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingViolations_ShouldReturnItems()
    {
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;
        var items = new List<OperatingGeofenceItemDto> { ItemFor(Guid.NewGuid()) };
        _repo.GetActiveGeofenceViolationsAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListActiveGeofenceViolationsQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TenantId.Should().Be(TestTenantId.Value);
    }

    [Fact]
    public async Task Handle_WhenNoViolations_ShouldReturnEmptyList()
    {
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;
        _repo.GetActiveGeofenceViolationsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<OperatingGeofenceItemDto>());

        var result = await Sut().Handle(new ListActiveGeofenceViolationsQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRetrievalFailedErrorCode()
    {
        _repo.GetActiveGeofenceViolationsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<OperatingGeofenceItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListActiveGeofenceViolationsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-1), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.OperatingGeofence.RetrievalFailed");
    }
}
