// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Validation
//  Layer:          Tests (Unit)
//  File:           CharacterizeWasteStreamCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Unit tests for CharacterizeWasteStreamCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Validation;

namespace PlantSafety.Tests.HazardousWasteManagement.Validation;

public sealed class CharacterizeWasteStreamCommandValidatorTests : UnitTestBase
{
    private readonly CharacterizeWasteStreamCommandValidator _validator = new();

    private static CharacterizeWasteStreamCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "Waste Oil Stream", "D001", "Ignitable", "EPA123456789", TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTenantId_ShouldFailValidation()
    {
        var command = ValidCommand() with { TenantId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TenantId);
    }

    [Fact]
    public void EmptyStreamName_ShouldFailValidation()
    {
        var command = ValidCommand() with { StreamName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.StreamName);
    }

    [Fact]
    public void StreamNameOverMaxLength_ShouldFailValidation()
    {
        var command = ValidCommand() with { StreamName = new string('x', 401) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.StreamName);
    }

    [Fact]
    public void EmptyEpaWasteCode_ShouldFailValidation()
    {
        var command = ValidCommand() with { EpaWasteCode = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EpaWasteCode);
    }

    [Fact]
    public void EmptyGeneratorEpaId_ShouldFailValidation()
    {
        var command = ValidCommand() with { GeneratorEpaId = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.GeneratorEpaId);
    }

    [Fact]
    public void EmptyPerformedByUserId_ShouldFailValidation()
    {
        var command = ValidCommand() with { PerformedByUserId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PerformedByUserId);
    }
}
