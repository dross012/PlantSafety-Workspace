// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Validation
//  Layer:          Tests (Unit)
//  File:           RecalculateWorkloadTrendCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper tests for RecalculateWorkloadTrendCommandValidator.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Validation;

public sealed class RecalculateWorkloadTrendCommandValidatorTests : UnitTestBase
{
    private readonly RecalculateWorkloadTrendCommandValidator _validator = new();

    private static RecalculateWorkloadTrendCommand ValidCommand() =>
        RecalculateWorkloadTrendCommand.FromDto(
            tenantId: TestTenantId.Value, cognitiveLoadLedgerId: Guid.NewGuid(),
            periodStartUtc: FixedUtcNow.AddHours(-1), periodEndUtc: FixedUtcNow.AddHours(1),
            requesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCognitiveLoadLedgerId_HasErrorForCognitiveLoadLedgerId()
    {
        var command = ValidCommand() with { CognitiveLoadLedgerId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.CognitiveLoadLedgerId);
    }

    [Fact]
    public void Validate_WithPeriodEndAtOrBeforePeriodStart_HasErrorForPeriodEndUtc()
    {
        var command = ValidCommand() with { PeriodEndUtc = ValidCommand().PeriodStartUtc };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PeriodEndUtc);
    }

    [Fact]
    public void Validate_WithEmptyRequesterUserId_HasErrorForRequesterUserId()
    {
        var command = ValidCommand() with { RequesterUserId = Guid.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RequesterUserId);
    }
}
