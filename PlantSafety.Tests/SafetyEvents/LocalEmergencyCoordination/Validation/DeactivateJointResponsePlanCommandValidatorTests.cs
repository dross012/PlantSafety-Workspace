// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Validation
//  Layer:          Tests (Unit)
//  File:           DeactivateJointResponsePlanCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation tests for DeactivateJointResponsePlanCommandValidator.
//                  Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Validation;

public sealed class DeactivateJointResponsePlanCommandValidatorTests : UnitTestBase
{
    private readonly DeactivateJointResponsePlanCommandValidator _validator = new();

    private static DeactivateJointResponsePlanCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        LocalEmergencyCoordinationId: Guid.NewGuid(),
        JointResponsePlanId: Guid.NewGuid(),
        DeactivatedByUserId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyJointResponsePlanId_HasError()
        => _validator.TestValidate(ValidCommand() with { JointResponsePlanId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.JointResponsePlanId);

    [Fact]
    public void Validate_WithEmptyDeactivatedByUserId_HasError()
        => _validator.TestValidate(ValidCommand() with { DeactivatedByUserId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.DeactivatedByUserId);

    [Fact]
    public void Validate_WithEmptyLocalEmergencyCoordinationId_HasError()
        => _validator.TestValidate(ValidCommand() with { LocalEmergencyCoordinationId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.LocalEmergencyCoordinationId);
}
