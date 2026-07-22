// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ReceivePPEStockCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.ReceivePPEStockCommand"/>.</summary>
public sealed class ReceivePPEStockCommandValidator : AbstractValidator<Commands.ReceivePPEStockCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ReceivePPEStockCommand"/>.</summary>
    public ReceivePPEStockCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEInventoryId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
