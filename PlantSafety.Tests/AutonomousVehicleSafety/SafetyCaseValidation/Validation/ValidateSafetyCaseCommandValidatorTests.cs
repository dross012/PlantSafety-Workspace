// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.SafetyCaseValidation.Validation
//  Layer:          Tests (Unit)
//  File:           ValidateSafetyCaseCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ValidateSafetyCaseCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.Validation;

namespace PlantSafety.Tests.AutonomousVehicleSafety.SafetyCaseValidation.Validation;

public sealed class ValidateSafetyCaseCommandValidatorTests : UnitTestBase
{
    private readonly ValidateSafetyCaseCommandValidator _validator = new();

    private static ValidateSafetyCaseCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        null, Guid.NewGuid(), "AnsiRia1508", "v1.0", 180, Guid.NewGuid(), TestUserId.Value);

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
    public void EmptyDocumentVersion_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { DocumentVersion = string.Empty });
        result.ShouldHaveValidationErrorFor(c => c.DocumentVersion);
    }

    [Fact]
    public void ZeroRevalidationCadenceDays_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { RevalidationCadenceDays = 0 });
        result.ShouldHaveValidationErrorFor(c => c.RevalidationCadenceDays);
    }

    [Fact]
    public void EmptyValidatedByUserId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { ValidatedByUserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.ValidatedByUserId);
    }
}
