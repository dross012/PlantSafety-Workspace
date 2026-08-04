// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RetireDischargeLimitCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.RetireDischargeLimitCommand"/>.</summary>
public sealed class RetireDischargeLimitCommandValidator : AbstractValidator<Commands.RetireDischargeLimitCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireDischargeLimitCommand"/>.</summary>
    public RetireDischargeLimitCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargeLimitId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}