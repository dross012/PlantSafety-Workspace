// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for FlagFailedChecklistItemCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Checklists.Validation;

/// <summary>Validates <see cref="Commands.FlagFailedChecklistItemCommand"/>.</summary>
public sealed class FlagFailedChecklistItemCommandValidator : AbstractValidator<Commands.FlagFailedChecklistItemCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagFailedChecklistItemCommand"/>.</summary>
    public FlagFailedChecklistItemCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.ChecklistCompletionId).NotEmpty();
        RuleFor(x => x.ChecklistTemplateItemId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1024);
    }
}
