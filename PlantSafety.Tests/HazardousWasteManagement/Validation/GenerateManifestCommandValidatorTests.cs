// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Validation
//  Layer:          Tests (Unit)
//  File:           GenerateManifestCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Unit tests for GenerateManifestCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Validation;

namespace PlantSafety.Tests.HazardousWasteManagement.Validation;

public sealed class GenerateManifestCommandValidatorTests : UnitTestBase
{
    private readonly GenerateManifestCommandValidator _validator = new();

    private static GenerateManifestCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "MAN-2026-000123", "EPA123456789", Guid.NewGuid(), TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyManifestNumber_ShouldFailValidation()
    {
        var command = ValidCommand() with { ManifestNumber = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManifestNumber);
    }

    [Fact]
    public void ManifestNumberOverMaxLength_ShouldFailValidation()
    {
        var command = ValidCommand() with { ManifestNumber = new string('9', 51) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManifestNumber);
    }

    [Fact]
    public void EmptyGeneratorEpaId_ShouldFailValidation()
    {
        var command = ValidCommand() with { GeneratorEpaId = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.GeneratorEpaId);
    }

    [Fact]
    public void EmptyWasteStreamId_ShouldFailValidation()
    {
        var command = ValidCommand() with { WasteStreamId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.WasteStreamId);
    }
}
