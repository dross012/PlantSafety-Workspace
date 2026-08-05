// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Commands
//  Layer:          Tests (Unit)
//  File:           SuspendAutonomousOperationCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SuspendAutonomousOperationCommandHandler. Covers the
//                  NotFound-severity regression fix (ErrorSeverity.NotFound must be set so the
//                  API maps missing assets to HTTP 404, not 500).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.ValueObjects.Ids;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates.AutonomousVehicleAsset;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Commands;

public sealed class SuspendAutonomousOperationCommandHandlerTests : UnitTestBase
{
    private readonly IAutonomousVehicleAssetRepository _repo = Substitute.For<IAutonomousVehicleAssetRepository>();

    private SuspendAutonomousOperationCommandHandler Sut() => new(_repo, NullLogger<SuspendAutonomousOperationCommandHandler>());

    private static Agg ActiveAsset()
    {
        var asset = Agg.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            assetTag: "AMR-001");
        asset.RecordModeTransition(AutonomyMode.Autonomous, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return asset;
    }

    private static SuspendAutonomousOperationCommand CommandFor(Guid assetId, string reason = "Repeated geofence violations") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        AutonomousVehicleAssetId: assetId,
        Reason: reason,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSuspendAsset_AndReturnSuccess()
    {
        var asset = ActiveAsset();
        _repo.GetByIdAsync(Arg.Any<AutonomousVehicleAssetId>(), Arg.Any<CancellationToken>()).Returns(asset);

        var result = await Sut().Handle(CommandFor(asset.AutonomousVehicleAssetId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Suspended");
        await _repo.Received(1).UpdateAsync(asset, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAssetDoesNotExist_ShouldReturnNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<AutonomousVehicleAssetId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleAsset.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AutonomousVehicleAsset record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAssetNotActive_ShouldReturnFailure()
    {
        var registeredAsset = Agg.Create(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, assetTag: "AMR-002");
        _repo.GetByIdAsync(Arg.Any<AutonomousVehicleAssetId>(), Arg.Any<CancellationToken>()).Returns(registeredAsset);

        var result = await Sut().Handle(CommandFor(registeredAsset.AutonomousVehicleAssetId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleAsset.SuspendFailed");
    }
}
