// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleIncident.Validation
//  Layer:          Tests (Unit)
//  File:           RecordAutonomousVehicleIncidentCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordAutonomousVehicleIncidentCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.Validation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleIncident.Validation;

public sealed class RecordAutonomousVehicleIncidentCommandValidatorTests : UnitTestBase
{
    private readonly RecordAutonomousVehicleIncidentCommandValidator _validator = new();

    private static RecordAutonomousVehicleIncidentCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, Guid.NewGuid(), "GeofenceBreach", "High", "Vehicle breached the north exclusion zone", TestUserId.Value);

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
    public void EmptyDescriptionSummary_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { DescriptionSummary = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.DescriptionSummary);
    }

    [Fact]
    public void DescriptionSummaryTooLong_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { DescriptionSummary = new string('A', 2001) });
        result.ShouldHaveValidationErrorFor(c => c.DescriptionSummary);
    }

    [Fact]
    public void EmptySeverityLevel_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { SeverityLevel = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.SeverityLevel);
    }
}
