// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleIncident.Handlers
//  Layer:          Tests (Unit)
//  File:           GetIncidentHistoryByAssetQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetIncidentHistoryByAssetQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Handlers;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Models;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Repositories;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleIncident.Handlers;

public sealed class GetIncidentHistoryByAssetQueryHandlerTests : UnitTestBase
{
    private readonly IAutonomousVehicleIncidentRepository _repo = Substitute.For<IAutonomousVehicleIncidentRepository>();

    private GetIncidentHistoryByAssetQueryHandler Sut() => new(_repo, NullLogger<GetIncidentHistoryByAssetQueryHandler>());

    private static AutonomousVehicleIncidentItemDto ItemFor(Guid id, Guid assetId) => new()
    {
        AutonomousVehicleIncidentId = id,
        TenantId = TestTenantId.Value,
        AutonomousVehicleAssetId = assetId,
        IncidentType = "GeofenceBreach",
        SeverityLevel = "High",
        Status = "Reported",
        SuspensionTriggered = false,
        OccurredAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingHistory_ShouldReturnItems()
    {
        var assetId = Guid.NewGuid();
        var items = new List<AutonomousVehicleIncidentItemDto> { ItemFor(Guid.NewGuid(), assetId) };
        _repo.GetIncidentHistoryByAssetIdAsync(TestTenantId.Value, assetId, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new GetIncidentHistoryByAssetQuery(TestTenantId.Value, assetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.AutonomousVehicleAssetId.Should().Be(assetId);
    }

    [Fact]
    public async Task Handle_WhenNoIncidents_ShouldReturnEmptyList()
    {
        _repo.GetIncidentHistoryByAssetIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<AutonomousVehicleIncidentItemDto>());

        var result = await Sut().Handle(new GetIncidentHistoryByAssetQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRetrievalFailedErrorCode()
    {
        _repo.GetIncidentHistoryByAssetIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<AutonomousVehicleIncidentItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetIncidentHistoryByAssetQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleIncident.RetrievalFailed");
    }
}
