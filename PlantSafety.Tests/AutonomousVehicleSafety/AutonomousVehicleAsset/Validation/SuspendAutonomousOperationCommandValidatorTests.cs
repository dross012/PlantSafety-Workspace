// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Validation
//  Layer:          Tests (Unit)
//  File:           SuspendAutonomousOperationCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SuspendAutonomousOperationCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.Validation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Validation;

public sealed class SuspendAutonomousOperationCommandValidatorTests : UnitTestBase
{
    private readonly SuspendAutonomousOperationCommandValidator _validator = new();

    private static SuspendAutonomousOperationCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Repeated geofence violations", TestUserId.Value);

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
    public void EmptyReason_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Reason = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.Reason);
    }

    [Fact]
    public void ReasonTooLong_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Reason = new string('A', 1001) });
        result.ShouldHaveValidationErrorFor(c => c.Reason);
    }
}
