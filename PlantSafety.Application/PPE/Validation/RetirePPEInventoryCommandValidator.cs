// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetirePPEInventoryCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.RetirePPEInventoryCommand"/>.</summary>
public sealed class RetirePPEInventoryCommandValidator : AbstractValidator<Commands.RetirePPEInventoryCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetirePPEInventoryCommand"/>.</summary>
    public RetirePPEInventoryCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEInventoryId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
