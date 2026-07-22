// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for DetermineEligibilityIncidentTrackingPerformanceEvaluationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.DetermineEligibilityIncidentTrackingPerformanceEvaluationCommand"/>.</summary>
public sealed class DetermineEligibilityIncidentTrackingPerformanceEvaluationCommandValidator : AbstractValidator<Commands.DetermineEligibilityIncidentTrackingPerformanceEvaluationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DetermineEligibilityIncidentTrackingPerformanceEvaluationCommand"/>.</summary>
    public DetermineEligibilityIncidentTrackingPerformanceEvaluationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.IncidentTrackingPerformanceEvaluationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.PerformanceScore).InclusiveBetween(0m, 100m);
    }
}