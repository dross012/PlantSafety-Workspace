// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Validation
//  Layer:          Tests (Unit)
//  File:           RegisterAutonomousVehicleAssetCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterAutonomousVehicleAssetCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.Validation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Validation;

public sealed class RegisterAutonomousVehicleAssetCommandValidatorTests : UnitTestBase
{
    private readonly RegisterAutonomousVehicleAssetCommandValidator _validator = new();

    private static RegisterAutonomousVehicleAssetCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, "AMR-001", "Amr", "SemiAutonomous", TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTenantId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { TenantId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.TenantId);
    }

    [Fact]
    public void EmptyAssetTag_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { AssetTag = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.AssetTag);
    }

    [Fact]
    public void AssetTagTooLong_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { AssetTag = new string('A', 201) });
        result.ShouldHaveValidationErrorFor(c => c.AssetTag);
    }

    [Fact]
    public void EmptyVehicleType_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { VehicleType = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.VehicleType);
    }

    [Fact]
    public void EmptyPerformedByUserId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { PerformedByUserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }
}
