// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Validation
//  Layer:          Tests (Unit)
//  File:           AssignExternalResponderLiaisonCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation tests for AssignExternalResponderLiaisonCommandValidator.
//                  Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Validation;

public sealed class AssignExternalResponderLiaisonCommandValidatorTests : UnitTestBase
{
    private readonly AssignExternalResponderLiaisonCommandValidator _validator = new();

    private static AssignExternalResponderLiaisonCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        LocalEmergencyCoordinationId: Guid.NewGuid(),
        IncidentId: Guid.NewGuid(),
        AgencyName: "County EMS",
        LiaisonUserId: Guid.NewGuid(),
        AssignedByUserId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyAgencyName_HasError()
        => _validator.TestValidate(ValidCommand() with { AgencyName = "" }).ShouldHaveValidationErrorFor(x => x.AgencyName);

    [Fact]
    public void Validate_WithEmptyLiaisonUserId_HasError()
        => _validator.TestValidate(ValidCommand() with { LiaisonUserId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.LiaisonUserId);

    [Fact]
    public void Validate_WithEmptyAssignedByUserId_HasError()
        => _validator.TestValidate(ValidCommand() with { AssignedByUserId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.AssignedByUserId);

    [Fact]
    public void Validate_WithAgencyNameExceedingMaxLength_HasError()
        => _validator.TestValidate(ValidCommand() with { AgencyName = new string('x', 201) }).ShouldHaveValidationErrorFor(x => x.AgencyName);
}
