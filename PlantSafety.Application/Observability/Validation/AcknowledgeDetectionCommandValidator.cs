// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for AcknowledgeDetectionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Observability.Validation;

/// <summary>Validates <see cref="Commands.AcknowledgeDetectionCommand"/>.</summary>
public sealed class AcknowledgeDetectionCommandValidator : AbstractValidator<Commands.AcknowledgeDetectionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AcknowledgeDetectionCommand"/>.</summary>
    public AcknowledgeDetectionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutomatedIncidentDetectionId).NotEmpty();
        RuleFor(x => x.AcknowledgedByUserId).NotEmpty();
    }
}
