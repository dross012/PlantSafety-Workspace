// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Validation
//  Layer:          Tests (Unit)
//  File:           AcknowledgeCognitiveRiskAlertCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper tests for AcknowledgeCognitiveRiskAlertCommandValidator.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Validation;

public sealed class AcknowledgeCognitiveRiskAlertCommandValidatorTests : UnitTestBase
{
    private readonly AcknowledgeCognitiveRiskAlertCommandValidator _validator = new();

    private static AcknowledgeCognitiveRiskAlertCommand ValidCommand() =>
        AcknowledgeCognitiveRiskAlertCommand.FromDto(
            tenantId: TestTenantId.Value, cognitiveRiskPredictionId: Guid.NewGuid(),
            acknowledgementNotes: "Reviewed.", requesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullAcknowledgementNotes_HasNoErrors()
    {
        var command = ValidCommand() with { AcknowledgementNotes = null };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCognitiveRiskPredictionId_HasErrorForCognitiveRiskPredictionId()
    {
        var command = ValidCommand() with { CognitiveRiskPredictionId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.CognitiveRiskPredictionId);
    }

    [Fact]
    public void Validate_WithAcknowledgementNotesExceedingMaxLength_HasErrorForAcknowledgementNotes()
    {
        var command = ValidCommand() with { AcknowledgementNotes = new string('a', 1001) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.AcknowledgementNotes);
    }

    [Fact]
    public void Validate_WithEmptyRequesterUserId_HasErrorForRequesterUserId()
    {
        var command = ValidCommand() with { RequesterUserId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RequesterUserId);
    }
}
