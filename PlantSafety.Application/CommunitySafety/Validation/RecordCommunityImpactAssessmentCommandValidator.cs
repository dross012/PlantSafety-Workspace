// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordCommunityImpactAssessmentCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.CommunitySafety.Validation;

/// <summary>Validates <see cref="Commands.RecordCommunityImpactAssessmentCommand"/>.</summary>
public sealed class RecordCommunityImpactAssessmentCommandValidator : AbstractValidator<Commands.RecordCommunityImpactAssessmentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordCommunityImpactAssessmentCommand"/>.</summary>
    public RecordCommunityImpactAssessmentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.IncidentId).NotEmpty();
        RuleFor(x => x.Severity).NotEmpty();
        RuleFor(x => x.AffectedArea).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.EstimatedPersonsAffected).GreaterThanOrEqualTo(0).When(x => x.EstimatedPersonsAffected.HasValue);
        RuleFor(x => x.AssessedByUserId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
