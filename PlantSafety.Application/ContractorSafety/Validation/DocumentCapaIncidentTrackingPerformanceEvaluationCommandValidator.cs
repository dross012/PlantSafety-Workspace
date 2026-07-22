// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for DocumentCapaIncidentTrackingPerformanceEvaluationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.DocumentCapaIncidentTrackingPerformanceEvaluationCommand"/>.</summary>
public sealed class DocumentCapaIncidentTrackingPerformanceEvaluationCommandValidator : AbstractValidator<Commands.DocumentCapaIncidentTrackingPerformanceEvaluationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DocumentCapaIncidentTrackingPerformanceEvaluationCommand"/>.</summary>
    public DocumentCapaIncidentTrackingPerformanceEvaluationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.IncidentTrackingPerformanceEvaluationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.CapaCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RootCauseSummary).NotEmpty().MaximumLength(1000);
    }
}