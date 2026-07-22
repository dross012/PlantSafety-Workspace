// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for VerifyZeroEnergyLotoProgramsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.VerifyZeroEnergyLotoProgramsCommand"/>.</summary>
public sealed class VerifyZeroEnergyLotoProgramsCommandValidator : AbstractValidator<Commands.VerifyZeroEnergyLotoProgramsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.VerifyZeroEnergyLotoProgramsCommand"/>.</summary>
    public VerifyZeroEnergyLotoProgramsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProgramsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.VerifiedByUserId).NotEmpty();
    }
}