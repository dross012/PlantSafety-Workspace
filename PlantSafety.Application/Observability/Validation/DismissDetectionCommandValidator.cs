// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for DismissDetectionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Observability.Validation;

/// <summary>Validates <see cref="Commands.DismissDetectionCommand"/>.</summary>
public sealed class DismissDetectionCommandValidator : AbstractValidator<Commands.DismissDetectionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DismissDetectionCommand"/>.</summary>
    public DismissDetectionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutomatedIncidentDetectionId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
