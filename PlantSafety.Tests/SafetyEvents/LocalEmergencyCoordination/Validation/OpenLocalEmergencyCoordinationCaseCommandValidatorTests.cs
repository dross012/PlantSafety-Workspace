// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Validation
//  Layer:          Tests (Unit)
//  File:           OpenLocalEmergencyCoordinationCaseCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation tests for OpenLocalEmergencyCoordinationCaseCommandValidator.
//                  Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Validation;

public sealed class OpenLocalEmergencyCoordinationCaseCommandValidatorTests : UnitTestBase
{
    private readonly OpenLocalEmergencyCoordinationCaseCommandValidator _validator = new();

    private static OpenLocalEmergencyCoordinationCaseCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        RequestedAtUtc: DateTime.UtcNow,
        ApiVersionToken: "v1",
        TenantId: Guid.NewGuid(),
        SiteId: null,
        IncidentId: null,
        CaseName: "Wildfire Mutual Aid Case",
        OpenedByUserId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyTenantId_HasError()
        => _validator.TestValidate(ValidCommand() with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Validate_WithEmptyOpenedByUserId_HasError()
        => _validator.TestValidate(ValidCommand() with { OpenedByUserId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.OpenedByUserId);

    [Fact]
    public void Validate_WithEmptyCaseName_HasError()
        => _validator.TestValidate(ValidCommand() with { CaseName = "" }).ShouldHaveValidationErrorFor(x => x.CaseName);

    [Fact]
    public void Validate_WithCaseNameExceedingMaxLength_HasError()
        => _validator.TestValidate(ValidCommand() with { CaseName = new string('x', 201) }).ShouldHaveValidationErrorFor(x => x.CaseName);
}
