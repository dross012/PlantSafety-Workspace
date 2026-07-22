// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordHazardAssessmentPermitToWorkCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordHazardAssessmentPermitToWorkCommand"/>.</summary>
public sealed class RecordHazardAssessmentPermitToWorkCommandValidator : AbstractValidator<Commands.RecordHazardAssessmentPermitToWorkCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordHazardAssessmentPermitToWorkCommand"/>.</summary>
    public RecordHazardAssessmentPermitToWorkCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitToWorkId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.HazardCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AtmosphericTestScore).InclusiveBetween(0m, 100m).When(x => x.AtmosphericTestScore.HasValue);
    }
}