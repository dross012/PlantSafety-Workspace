// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Validation
//  Layer:          Tests (Unit)
//  File:           FlagManifestDiscrepancyCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Unit tests for FlagManifestDiscrepancyCommandValidator rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.Validation;

namespace PlantSafety.Tests.HazardousWasteManagement.Validation;

public sealed class FlagManifestDiscrepancyCommandValidatorTests : UnitTestBase
{
    private readonly FlagManifestDiscrepancyCommandValidator _validator = new();

    private static FlagManifestDiscrepancyCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Quantity mismatch on line 2", TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyHazardousWasteManifestId_ShouldFailValidation()
    {
        var command = ValidCommand() with { HazardousWasteManifestId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.HazardousWasteManifestId);
    }

    [Fact]
    public void EmptyDiscrepancyReason_ShouldFailValidation()
    {
        var command = ValidCommand() with { DiscrepancyReason = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DiscrepancyReason);
    }

    [Fact]
    public void DiscrepancyReasonOverMaxLength_ShouldFailValidation()
    {
        var command = ValidCommand() with { DiscrepancyReason = new string('x', 2001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.DiscrepancyReason);
    }
}
