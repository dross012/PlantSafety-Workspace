// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: FluentValidation validator for CreateChecklistCompletionCommand. Enforces invariants and normalization boundaries only; never calls repositories or external services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Checklists.Validation;

/// <summary>Validates <see cref="Commands.CreateChecklistCompletionCommand"/>.</summary>
public sealed class CreateChecklistCompletionCommandValidator : AbstractValidator<Commands.CreateChecklistCompletionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateChecklistCompletionCommand"/>.</summary>
    public CreateChecklistCompletionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.ChecklistTemplateId).NotEmpty();
        RuleFor(x => x.ItemsTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ItemsPassed).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ItemsFailed).GreaterThanOrEqualTo(0);
    }
}