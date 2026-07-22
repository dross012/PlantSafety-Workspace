// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordCultureAssessmentScoreCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyCulture.Validation;

/// <summary>Validates <see cref="Commands.RecordCultureAssessmentScoreCommand"/>.</summary>
public sealed class RecordCultureAssessmentScoreCommandValidator : AbstractValidator<Commands.RecordCultureAssessmentScoreCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordCultureAssessmentScoreCommand"/>.</summary>
    public RecordCultureAssessmentScoreCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.CultureAssessmentId).NotEmpty();
        RuleFor(x => x.OverallCultureScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.LeadershipEngagementScore).InclusiveBetween(0m, 100m).When(x => x.LeadershipEngagementScore.HasValue);
    }
}
