// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordAssessmentErgonomicsAndMaterialHandlingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordAssessmentErgonomicsAndMaterialHandlingCommand"/>.</summary>
public sealed class RecordAssessmentErgonomicsAndMaterialHandlingCommandValidator : AbstractValidator<Commands.RecordAssessmentErgonomicsAndMaterialHandlingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordAssessmentErgonomicsAndMaterialHandlingCommand"/>.</summary>
    public RecordAssessmentErgonomicsAndMaterialHandlingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ErgonomicsAndMaterialHandlingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.RiskFactorCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RiskScore).InclusiveBetween(0m, 100m);
    }
}