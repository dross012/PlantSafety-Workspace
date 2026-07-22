// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for FlagPPEInventoryForReplacementCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.FlagPPEInventoryForReplacementCommand"/>.</summary>
public sealed class FlagPPEInventoryForReplacementCommandValidator : AbstractValidator<Commands.FlagPPEInventoryForReplacementCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagPPEInventoryForReplacementCommand"/>.</summary>
    public FlagPPEInventoryForReplacementCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEInventoryId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
