// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Commands
//  Layer:          Tests (Unit)
//  File:           RegisterAutonomousVehicleAssetCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterAutonomousVehicleAssetCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates.AutonomousVehicleAsset;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Commands;

public sealed class RegisterAutonomousVehicleAssetCommandHandlerTests : UnitTestBase
{
    private readonly IAutonomousVehicleAssetRepository _repo = Substitute.For<IAutonomousVehicleAssetRepository>();

    private RegisterAutonomousVehicleAssetCommandHandler Sut() => new(_repo, NullLogger<RegisterAutonomousVehicleAssetCommandHandler>());

    private static RegisterAutonomousVehicleAssetCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AssetTag: "AMR-042",
        VehicleType: "Amr",
        AutonomyLevel: "SemiAutonomous",
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistAsset_AndReturnSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Registered");
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.AssetTag == "AMR-042"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedVehicleType_ShouldDefaultToAmr()
    {
        var command = ValidCommand() with { VehicleType = "NotARealType" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.VehicleType.ToString() == "Amr"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRegisterFailedErrorCode()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleAsset.RegisterFailed");
    }
}
