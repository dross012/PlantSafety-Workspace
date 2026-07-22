// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for IssuePPEUnitsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.IssuePPEUnitsCommand"/>.</summary>
public sealed class IssuePPEUnitsCommandValidator : AbstractValidator<Commands.IssuePPEUnitsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.IssuePPEUnitsCommand"/>.</summary>
    public IssuePPEUnitsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEInventoryId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ReorderThreshold).GreaterThanOrEqualTo(0);
    }
}
