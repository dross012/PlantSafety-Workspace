// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Validation
//  Layer:          Tests (Unit)
//  File:           RecordAutonomyModeTransitionCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordAutonomyModeTransitionCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.Validation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Validation;

public sealed class RecordAutonomyModeTransitionCommandValidatorTests : UnitTestBase
{
    private readonly RecordAutonomyModeTransitionCommandValidator _validator = new();

    private static RecordAutonomyModeTransitionCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Autonomous", TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyAutonomousVehicleAssetId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { AutonomousVehicleAssetId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.AutonomousVehicleAssetId);
    }

    [Fact]
    public void EmptyToMode_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { ToMode = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.ToMode);
    }

    [Fact]
    public void EmptyPerformedByUserId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { PerformedByUserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }
}
