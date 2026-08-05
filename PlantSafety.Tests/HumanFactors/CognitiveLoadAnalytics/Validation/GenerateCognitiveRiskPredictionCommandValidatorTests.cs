// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Validation
//  Layer:          Tests (Unit)
//  File:           GenerateCognitiveRiskPredictionCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper tests for GenerateCognitiveRiskPredictionCommandValidator.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Validation;

public sealed class GenerateCognitiveRiskPredictionCommandValidatorTests : UnitTestBase
{
    private readonly GenerateCognitiveRiskPredictionCommandValidator _validator = new();

    private static GenerateCognitiveRiskPredictionCommand ValidCommand() =>
        GenerateCognitiveRiskPredictionCommand.FromDto(
            tenantId: TestTenantId.Value, siteId: null, workerUserId: TestUserId.Value,
            sourceLedgerId: Guid.NewGuid(), modelVersion: "v1.0.0", requesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyWorkerUserId_HasErrorForWorkerUserId()
    {
        var command = ValidCommand() with { WorkerUserId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.WorkerUserId);
    }

    [Fact]
    public void Validate_WithEmptySourceLedgerId_HasErrorForSourceLedgerId()
    {
        var command = ValidCommand() with { SourceLedgerId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.SourceLedgerId);
    }

    [Fact]
    public void Validate_WithEmptyModelVersion_HasErrorForModelVersion()
    {
        var command = ValidCommand() with { ModelVersion = "" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ModelVersion);
    }

    [Fact]
    public void Validate_WithModelVersionExceedingMaxLength_HasErrorForModelVersion()
    {
        var command = ValidCommand() with { ModelVersion = new string('v', 101) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ModelVersion);
    }
}
