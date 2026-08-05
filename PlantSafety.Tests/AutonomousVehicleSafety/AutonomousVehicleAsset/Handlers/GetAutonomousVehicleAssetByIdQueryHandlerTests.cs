// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAutonomousVehicleAssetByIdQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAutonomousVehicleAssetByIdQueryHandler. The read-model repository
//                  interface returns a nullable DTO and the handler always wraps the result in
//                  Result.Success (the controller declares no 404 ProducesResponseType for this
//                  endpoint, so a missing record surfaces as 200 OK with a null payload -- verified
//                  here rather than assuming the DigitalTwin-style explicit-NotFound convention).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Handlers;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Models;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Repositories;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Handlers;

public sealed class GetAutonomousVehicleAssetByIdQueryHandlerTests : UnitTestBase
{
    private readonly IAutonomousVehicleAssetRepository _repo = Substitute.For<IAutonomousVehicleAssetRepository>();

    private GetAutonomousVehicleAssetByIdQueryHandler Sut() => new(_repo, NullLogger<GetAutonomousVehicleAssetByIdQueryHandler>());

    private static AutonomousVehicleAssetItemDto ItemFor(Guid id, Guid tenantId) => new()
    {
        AutonomousVehicleAssetId = id,
        TenantId = tenantId,
        AssetTag = "AMR-001",
        VehicleType = "Amr",
        AutonomyLevel = "SemiAutonomous",
        CurrentAutonomyMode = "Manual",
        ModeTransitionCount = 0,
        Status = "Registered",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenAssetExistsForTenant_ShouldReturnItem()
    {
        var assetId = Guid.NewGuid();
        var item = ItemFor(assetId, TestTenantId.Value);
        _repo.GetAutonomousVehicleAssetByIdAsync(TestTenantId.Value, assetId, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new GetAutonomousVehicleAssetByIdQuery(TestTenantId.Value, assetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AssetTag.Should().Be("AMR-001");
    }

    [Fact]
    public async Task Handle_WhenAssetDoesNotExist_ShouldReturnSuccessWithNullValue()
    {
        _repo.GetAutonomousVehicleAssetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AutonomousVehicleAssetItemDto?)null);

        var result = await Sut().Handle(new GetAutonomousVehicleAssetByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRetrievalFailedErrorCode()
    {
        _repo.GetAutonomousVehicleAssetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<AutonomousVehicleAssetItemDto?>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetAutonomousVehicleAssetByIdQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleAsset.RetrievalFailed");
    }
}
