// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for PublishCultureAssessmentReportCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyCulture.Validation;

/// <summary>Validates <see cref="Commands.PublishCultureAssessmentReportCommand"/>.</summary>
public sealed class PublishCultureAssessmentReportCommandValidator : AbstractValidator<Commands.PublishCultureAssessmentReportCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PublishCultureAssessmentReportCommand"/>.</summary>
    public PublishCultureAssessmentReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.CultureAssessmentId).NotEmpty();
        RuleFor(x => x.Summary).MaximumLength(4096);
    }
}
