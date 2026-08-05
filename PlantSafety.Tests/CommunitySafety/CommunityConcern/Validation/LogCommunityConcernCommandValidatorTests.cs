// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityConcern.Validation
//  Layer:          Tests (Unit)
//  File:           LogCommunityConcernCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for LogCommunityConcernCommandValidator rules.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.Validation;

namespace PlantSafety.Tests.CommunitySafety.CommunityConcern.Validation;

public sealed class LogCommunityConcernCommandValidatorTests : UnitTestBase
{
    private readonly LogCommunityConcernCommandValidator _validator = new();

    private static LogCommunityConcernCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
        "Odor", "Medium", "Tank Farm", "Strong odor reported near the fence line.", null, null, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptySiteId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { SiteId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.SiteId);
    }

    [Fact]
    public void EmptyDescription_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Description = "" });
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void DescriptionTooLong_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Description = new string('x', 4001) });
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void EmptyFacilityArea_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { FacilityArea = "" });
        result.ShouldHaveValidationErrorFor(c => c.FacilityArea);
    }

    [Fact]
    public void EmptySeverity_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Severity = "" });
        result.ShouldHaveValidationErrorFor(c => c.Severity);
    }
}
