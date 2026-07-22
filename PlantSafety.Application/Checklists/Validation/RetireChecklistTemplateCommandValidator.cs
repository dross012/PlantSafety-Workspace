// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireChecklistTemplateCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Checklists.Validation;

/// <summary>Validates <see cref="Commands.RetireChecklistTemplateCommand"/>.</summary>
public sealed class RetireChecklistTemplateCommandValidator : AbstractValidator<Commands.RetireChecklistTemplateCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireChecklistTemplateCommand"/>.</summary>
    public RetireChecklistTemplateCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.ChecklistTemplateId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(512);
    }
}
