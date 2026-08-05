// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityConcern.Validation
//  Layer:          Tests (Unit)
//  File:           ResolveCommunityConcernCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ResolveCommunityConcernCommandValidator rules.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.Validation;

namespace PlantSafety.Tests.CommunitySafety.CommunityConcern.Validation;

public sealed class ResolveCommunityConcernCommandValidatorTests : UnitTestBase
{
    private readonly ResolveCommunityConcernCommandValidator _validator = new();

    private static ResolveCommunityConcernCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
        Guid.NewGuid(), "Root cause corrected.", TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyCommunityConcernId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { CommunityConcernId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.CommunityConcernId);
    }

    [Fact]
    public void EmptyResolutionNotes_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { ResolutionNotes = "" });
        result.ShouldHaveValidationErrorFor(c => c.ResolutionNotes);
    }

    [Fact]
    public void ResolutionNotesTooLong_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { ResolutionNotes = new string('x', 4001) });
        result.ShouldHaveValidationErrorFor(c => c.ResolutionNotes);
    }
}
