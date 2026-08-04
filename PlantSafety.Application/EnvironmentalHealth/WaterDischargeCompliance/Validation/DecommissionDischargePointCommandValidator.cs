// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DecommissionDischargePointCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.DecommissionDischargePointCommand"/>.</summary>
public sealed class DecommissionDischargePointCommandValidator : AbstractValidator<Commands.DecommissionDischargePointCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DecommissionDischargePointCommand"/>.</summary>
    public DecommissionDischargePointCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePointId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}