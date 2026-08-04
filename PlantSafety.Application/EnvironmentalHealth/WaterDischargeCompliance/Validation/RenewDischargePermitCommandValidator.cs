// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RenewDischargePermitCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.RenewDischargePermitCommand"/>.</summary>
public sealed class RenewDischargePermitCommandValidator : AbstractValidator<Commands.RenewDischargePermitCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RenewDischargePermitCommand"/>.</summary>
    public RenewDischargePermitCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePermitId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}