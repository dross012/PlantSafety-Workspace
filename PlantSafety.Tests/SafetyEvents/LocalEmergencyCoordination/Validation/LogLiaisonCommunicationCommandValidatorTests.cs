// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Validation
//  Layer:          Tests (Unit)
//  File:           LogLiaisonCommunicationCommandValidatorTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation tests for LogLiaisonCommunicationCommandValidator. Isolated,
//                  deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Validation;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Validation;

public sealed class LogLiaisonCommunicationCommandValidatorTests : UnitTestBase
{
    private readonly LogLiaisonCommunicationCommandValidator _validator = new();

    private static LogLiaisonCommunicationCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: Guid.NewGuid(),
        CausationId: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        LocalEmergencyCoordinationId: Guid.NewGuid(),
        ExternalResponderLiaisonId: Guid.NewGuid(),
        Message: "Requesting additional resources.",
        LoggedByUserId: Guid.NewGuid());

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
        => _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_WithEmptyExternalResponderLiaisonId_HasError()
        => _validator.TestValidate(ValidCommand() with { ExternalResponderLiaisonId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.ExternalResponderLiaisonId);

    [Fact]
    public void Validate_WithEmptyMessage_HasError()
        => _validator.TestValidate(ValidCommand() with { Message = "" }).ShouldHaveValidationErrorFor(x => x.Message);

    [Fact]
    public void Validate_WithMessageExceedingMaxLength_HasError()
        => _validator.TestValidate(ValidCommand() with { Message = new string('x', 2001) }).ShouldHaveValidationErrorFor(x => x.Message);
}
