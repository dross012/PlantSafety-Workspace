// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for AddChecklistTemplateItemCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Checklists.Validation;

/// <summary>Validates <see cref="Commands.AddChecklistTemplateItemCommand"/>.</summary>
public sealed class AddChecklistTemplateItemCommandValidator : AbstractValidator<Commands.AddChecklistTemplateItemCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AddChecklistTemplateItemCommand"/>.</summary>
    public AddChecklistTemplateItemCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.ChecklistTemplateId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(512);
        RuleFor(x => x.RegulatoryReference).MaximumLength(256);
    }
}
