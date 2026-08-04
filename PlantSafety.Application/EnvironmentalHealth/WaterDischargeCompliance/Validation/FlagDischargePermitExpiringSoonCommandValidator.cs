// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for FlagDischargePermitExpiringSoonCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.FlagDischargePermitExpiringSoonCommand"/>.</summary>
public sealed class FlagDischargePermitExpiringSoonCommandValidator : AbstractValidator<Commands.FlagDischargePermitExpiringSoonCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagDischargePermitExpiringSoonCommand"/>.</summary>
    public FlagDischargePermitExpiringSoonCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePermitId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}