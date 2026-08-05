// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.OperatingGeofence.Validation
//  Layer:          Tests (Unit)
//  File:           DefineOperatingGeofenceCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DefineOperatingGeofenceCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.Validation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.OperatingGeofence.Validation;

public sealed class DefineOperatingGeofenceCommandValidatorTests : UnitTestBase
{
    private readonly DefineOperatingGeofenceCommandValidator _validator = new();

    private static DefineOperatingGeofenceCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, "North Yard Exclusion", "ExclusionZone", "POLYGON((0 0,1 0,1 1,0 1,0 0))", null, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyGeofenceName_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { GeofenceName = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.GeofenceName);
    }

    [Fact]
    public void EmptyBoundaryDefinition_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { BoundaryDefinition = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.BoundaryDefinition);
    }

    [Fact]
    public void NegativeMaxSpeedKph_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { MaxSpeedKph = -5m });
        result.ShouldHaveValidationErrorFor(c => c.MaxSpeedKph);
    }

    [Fact]
    public void NullMaxSpeedKph_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { MaxSpeedKph = null });
        result.ShouldNotHaveValidationErrorFor(c => c.MaxSpeedKph);
    }
}
