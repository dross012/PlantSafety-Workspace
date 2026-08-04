// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for ReactivateDischargePointCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.ReactivateDischargePointCommand"/>.</summary>
public sealed class ReactivateDischargePointCommandValidator : AbstractValidator<Commands.ReactivateDischargePointCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ReactivateDischargePointCommand"/>.</summary>
    public ReactivateDischargePointCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePointId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}