// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: FluentValidation validator for CreateChecklistTemplateCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Checklists.Validation;

/// <summary>Validates <see cref="Commands.CreateChecklistTemplateCommand"/>.</summary>
public sealed class CreateChecklistTemplateCommandValidator : AbstractValidator<Commands.CreateChecklistTemplateCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateChecklistTemplateCommand"/>.</summary>
    public CreateChecklistTemplateCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.TemplateTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.ItemCount).GreaterThanOrEqualTo(0);
    }
}