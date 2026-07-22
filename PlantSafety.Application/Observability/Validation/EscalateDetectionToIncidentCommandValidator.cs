// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for EscalateDetectionToIncidentCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Observability.Validation;

/// <summary>Validates <see cref="Commands.EscalateDetectionToIncidentCommand"/>.</summary>
public sealed class EscalateDetectionToIncidentCommandValidator : AbstractValidator<Commands.EscalateDetectionToIncidentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.EscalateDetectionToIncidentCommand"/>.</summary>
    public EscalateDetectionToIncidentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutomatedIncidentDetectionId).NotEmpty();
        RuleFor(x => x.IncidentReferenceId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
