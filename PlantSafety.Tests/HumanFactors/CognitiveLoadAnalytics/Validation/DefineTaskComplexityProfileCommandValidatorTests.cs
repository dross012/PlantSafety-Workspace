// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Validation
//  Layer:          Tests (Unit)
//  File:           DefineTaskComplexityProfileCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation.TestHelper tests for DefineTaskComplexityProfileCommandValidator.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Validation;

public sealed class DefineTaskComplexityProfileCommandValidatorTests : UnitTestBase
{
    private readonly DefineTaskComplexityProfileCommandValidator _validator = new();

    private static DefineTaskComplexityProfileCommand ValidCommand() =>
        DefineTaskComplexityProfileCommand.FromDto(
            tenantId: TestTenantId.Value, siteId: null, shiftType: "Day", shiftDateUtc: FixedUtcNow,
            taskCode: "CTRL-ROOM-STARTUP", taskName: "Control Room Startup", complexityScore: 7,
            description: "desc", requesterUserId: TestUserId.Value);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithUnrecognizedShiftType_HasErrorForShiftType()
    {
        var command = ValidCommand() with { ShiftType = "Midday" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ShiftType);
    }

    [Fact]
    public void Validate_WithEmptyTaskCode_HasErrorForTaskCode()
    {
        var command = ValidCommand() with { TaskCode = "" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.TaskCode);
    }

    [Fact]
    public void Validate_WithEmptyTaskName_HasErrorForTaskName()
    {
        var command = ValidCommand() with { TaskName = "" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.TaskName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Validate_WithComplexityScoreOutOfRange_HasErrorForComplexityScore(int score)
    {
        var command = ValidCommand() with { ComplexityScore = score };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ComplexityScore);
    }

    [Fact]
    public void Validate_WithDescriptionExceedingMaxLength_HasErrorForDescription()
    {
        var command = ValidCommand() with { Description = new string('a', 1001) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Description);
    }
}
