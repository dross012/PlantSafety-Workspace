// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Commands
//  Layer:          Tests (Unit)
//  File:           RecordAutonomyModeTransitionCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordAutonomyModeTransitionCommandHandler. Covers the
//                  NotFound-severity regression fix (ErrorSeverity.NotFound must be set so the
//                  API maps missing assets to HTTP 404, not 500).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.ValueObjects.Ids;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates.AutonomousVehicleAsset;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Commands;

public sealed class RecordAutonomyModeTransitionCommandHandlerTests : UnitTestBase
{
    private readonly IAutonomousVehicleAssetRepository _repo = Substitute.For<IAutonomousVehicleAssetRepository>();

    private RecordAutonomyModeTransitionCommandHandler Sut() => new(_repo, NullLogger<RecordAutonomyModeTransitionCommandHandler>());

    private static Agg RegisteredAsset() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        assetTag: "AMR-001");

    private static RecordAutonomyModeTransitionCommand CommandFor(Guid assetId, string toMode = "Autonomous") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        AutonomousVehicleAssetId: assetId,
        ToMode: toMode,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRecordTransition_AndReturnSuccess()
    {
        var asset = RegisteredAsset();
        _repo.GetByIdAsync(Arg.Any<AutonomousVehicleAssetId>(), Arg.Any<CancellationToken>()).Returns(asset);

        var result = await Sut().Handle(CommandFor(asset.AutonomousVehicleAssetId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
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
    public async Task Handle_WithUnrecognizedToMode_ShouldDefaultToAutonomous()
    {
        var asset = RegisteredAsset();
        _repo.GetByIdAsync(Arg.Any<AutonomousVehicleAssetId>(), Arg.Any<CancellationToken>()).Returns(asset);

        var result = await Sut().Handle(CommandFor(asset.AutonomousVehicleAssetId.Value, "NotARealMode"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        asset.CurrentAutonomyMode.ToString().Should().Be("Autonomous");
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateThrows_ShouldReturnFailure()
    {
        var asset = RegisteredAsset();
        _repo.GetByIdAsync(Arg.Any<AutonomousVehicleAssetId>(), Arg.Any<CancellationToken>()).Returns(asset);
        _repo.UpdateAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(CommandFor(asset.AutonomousVehicleAssetId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleAsset.RecordModeTransitionFailed");
    }
}
