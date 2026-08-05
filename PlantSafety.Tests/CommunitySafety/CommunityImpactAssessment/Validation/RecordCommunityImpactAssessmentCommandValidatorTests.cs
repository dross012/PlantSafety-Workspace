// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityImpactAssessment.Validation
//  Layer:          Tests (Unit)
//  File:           RecordCommunityImpactAssessmentCommandValidatorTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordCommunityImpactAssessmentCommandValidator rules.
// ============================================================================
using FluentValidation.TestHelper;
using PlantSafety.Application.CommunitySafety.Commands;
using PlantSafety.Application.CommunitySafety.Validation;

namespace PlantSafety.Tests.CommunitySafety.CommunityImpactAssessment.Validation;

public sealed class RecordCommunityImpactAssessmentCommandValidatorTests : UnitTestBase
{
    private readonly RecordCommunityImpactAssessmentCommandValidator _validator = new();

    private static RecordCommunityImpactAssessmentCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
        Guid.NewGuid(), "Severe", "North Ridge", "Vapor cloud drifted toward residential area.",
        25, TestUserId.Value, TestUserId.Value);

    [Fact]
    public void ValidCommand_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyIncidentId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { IncidentId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.IncidentId);
    }

    [Fact]
    public void EmptySeverity_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { Severity = "" });
        result.ShouldHaveValidationErrorFor(c => c.Severity);
    }

    [Fact]
    public void NegativeEstimatedPersonsAffected_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { EstimatedPersonsAffected = -1 });
        result.ShouldHaveValidationErrorFor(c => c.EstimatedPersonsAffected);
    }

    [Fact]
    public void NullEstimatedPersonsAffected_ShouldPassValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { EstimatedPersonsAffected = null });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyAssessedByUserId_ShouldFailValidation()
    {
        var result = _validator.TestValidate(ValidCommand() with { AssessedByUserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(c => c.AssessedByUserId);
    }
}
