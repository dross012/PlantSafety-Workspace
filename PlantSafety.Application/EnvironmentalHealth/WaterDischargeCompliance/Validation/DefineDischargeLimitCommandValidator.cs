// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DefineDischargeLimitCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.DefineDischargeLimitCommand"/>.</summary>
public sealed class DefineDischargeLimitCommandValidator : AbstractValidator<Commands.DefineDischargeLimitCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DefineDischargeLimitCommand"/>.</summary>
    public DefineDischargeLimitCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePointId).NotEmpty();
        RuleFor(x => x.DischargePermitId).NotEmpty();
        RuleFor(x => x.LimitValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PermitConditionReference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}