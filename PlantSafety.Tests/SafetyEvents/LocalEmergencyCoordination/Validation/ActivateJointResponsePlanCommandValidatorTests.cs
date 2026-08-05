// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Validation
//  Layer:          Tests (Unit)
//  File:           ActivateJointResponsePlanCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation tests for ActivateJointResponsePlanCommandValidator. Isolated,
//                  deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Validation;

public sealed class ActivateJointResponsePlanCommandValidatorTests : UnitTestBase
{
    private readonly ActivateJointResponsePlanCommandValidator _validator = new();

    private static ActivateJointResponsePlanCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        LocalEmergencyCoordinationId: Guid.NewGuid(),
        IncidentId: Guid.NewGuid(),
        PlanName: "Joint Wildfire Response Plan",
        ActivatedByUserId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyPlanName_HasError()
        => _validator.TestValidate(ValidCommand() with { PlanName = "" }).ShouldHaveValidationErrorFor(x => x.PlanName);

    [Fact]
    public void Validate_WithPlanNameExceedingMaxLength_HasError()
        => _validator.TestValidate(ValidCommand() with { PlanName = new string('x', 201) }).ShouldHaveValidationErrorFor(x => x.PlanName);

    [Fact]
    public void Validate_WithEmptyActivatedByUserId_HasError()
        => _validator.TestValidate(ValidCommand() with { ActivatedByUserId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.ActivatedByUserId);

    [Fact]
    public void Validate_WithNullIncidentId_HasNoErrors()
        => _validator.TestValidate(ValidCommand() with { IncidentId = null }).ShouldNotHaveAnyValidationErrors();
}
