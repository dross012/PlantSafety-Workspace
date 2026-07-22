// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireTwoHandControlsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RetireTwoHandControlsCommand"/>.</summary>
public sealed class RetireTwoHandControlsCommandValidator : AbstractValidator<Commands.RetireTwoHandControlsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireTwoHandControlsCommand"/>.</summary>
    public RetireTwoHandControlsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TwoHandControlsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}