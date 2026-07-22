// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for VerifyZeroEnergyLotoSystemsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.VerifyZeroEnergyLotoSystemsCommand"/>.</summary>
public sealed class VerifyZeroEnergyLotoSystemsCommandValidator : AbstractValidator<Commands.VerifyZeroEnergyLotoSystemsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.VerifyZeroEnergyLotoSystemsCommand"/>.</summary>
    public VerifyZeroEnergyLotoSystemsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoSystemsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.VerifiedByUserId).NotEmpty();
    }
}