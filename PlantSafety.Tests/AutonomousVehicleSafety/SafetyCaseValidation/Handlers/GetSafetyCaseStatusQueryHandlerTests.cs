// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.SafetyCaseValidation.Handlers
//  Layer:          Tests (Unit)
//  File:           GetSafetyCaseStatusQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetSafetyCaseStatusQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Handlers;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Models;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Repositories;

namespace PlantSafety.Tests.AutonomousVehicleSafety.SafetyCaseValidation.Handlers;

public sealed class GetSafetyCaseStatusQueryHandlerTests : UnitTestBase
{
    private readonly ISafetyCaseValidationRepository _repo = Substitute.For<ISafetyCaseValidationRepository>();

    private GetSafetyCaseStatusQueryHandler Sut() => new(_repo, NullLogger<GetSafetyCaseStatusQueryHandler>());

    private static SafetyCaseValidationItemDto ItemFor(Guid id, Guid assetId) => new()
    {
        SafetyCaseValidationId = id,
        TenantId = TestTenantId.Value,
        AutonomousVehicleAssetId = assetId,
        StandardReference = "AnsiRia1508",
        DocumentVersion = "v1.0",
        Status = "Validated",
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WhenSafetyCaseExists_ShouldReturnItem()
    {
        var assetId = Guid.NewGuid();
        var item = ItemFor(Guid.NewGuid(), assetId);
        _repo.GetSafetyCaseStatusByAssetIdAsync(TestTenantId.Value, assetId, Arg.Any<CancellationToken>()).Returns(item);

        var result = await Sut().Handle(new GetSafetyCaseStatusQuery(TestTenantId.Value, assetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be("Validated");
    }

    [Fact]
    public async Task Handle_WhenNoSafetyCaseExists_ShouldReturnSuccessWithNullValue()
    {
        _repo.GetSafetyCaseStatusByAssetIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SafetyCaseValidationItemDto?)null);

        var result = await Sut().Handle(new GetSafetyCaseStatusQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRetrievalFailedErrorCode()
    {
        _repo.GetSafetyCaseStatusByAssetIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<SafetyCaseValidationItemDto?>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new GetSafetyCaseStatusQuery(TestTenantId.Value, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.SafetyCaseValidation.RetrievalFailed");
    }
}
