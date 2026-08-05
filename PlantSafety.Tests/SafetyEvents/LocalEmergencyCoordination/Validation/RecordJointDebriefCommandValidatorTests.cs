// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Validation
//  Layer:          Tests (Unit)
//  File:           RecordJointDebriefCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation tests for RecordJointDebriefCommandValidator. Isolated,
//                  deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Validation;

public sealed class RecordJointDebriefCommandValidatorTests : UnitTestBase
{
    private readonly RecordJointDebriefCommandValidator _validator = new();

    private static RecordJointDebriefCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        LocalEmergencyCoordinationId: Guid.NewGuid(),
        IncidentId: Guid.NewGuid(),
        AgenciesInvolved: "Metro Fire, County EMS",
        Summary: "Response was well coordinated.",
        RecordedByUserId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyAgenciesInvolved_HasError()
        => _validator.TestValidate(ValidCommand() with { AgenciesInvolved = "" }).ShouldHaveValidationErrorFor(x => x.AgenciesInvolved);

    [Fact]
    public void Validate_WithEmptySummary_HasError()
        => _validator.TestValidate(ValidCommand() with { Summary = "" }).ShouldHaveValidationErrorFor(x => x.Summary);

    [Fact]
    public void Validate_WithSummaryExceedingMaxLength_HasError()
        => _validator.TestValidate(ValidCommand() with { Summary = new string('x', 4001) }).ShouldHaveValidationErrorFor(x => x.Summary);

    [Fact]
    public void Validate_WithAgenciesInvolvedExceedingMaxLength_HasError()
        => _validator.TestValidate(ValidCommand() with { AgenciesInvolved = new string('x', 501) }).ShouldHaveValidationErrorFor(x => x.AgenciesInvolved);
}
