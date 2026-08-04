// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterDischargePointCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.RegisterDischargePointCommand"/>.</summary>
public sealed class RegisterDischargePointCommandValidator : AbstractValidator<Commands.RegisterDischargePointCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterDischargePointCommand"/>.</summary>
    public RegisterDischargePointCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePermitId).NotEmpty();
        RuleFor(x => x.PointCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReceivingWaterBody).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}