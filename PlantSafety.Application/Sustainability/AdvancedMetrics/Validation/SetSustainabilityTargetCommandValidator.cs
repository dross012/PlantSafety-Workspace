// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           SetSustainabilityTargetCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for SetSustainabilityTargetCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Validation;

/// <summary>Validates <see cref="Commands.SetSustainabilityTargetCommand"/>.</summary>
public sealed class SetSustainabilityTargetCommandValidator : AbstractValidator<Commands.SetSustainabilityTargetCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SetSustainabilityTargetCommand"/>.</summary>
    public SetSustainabilityTargetCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ReportingPeriodEndUtc).GreaterThan(x => x.ReportingPeriodStartUtc);
        RuleFor(x => x.TargetType).NotEmpty().Must(s => Enum.TryParse<Domain.Sustainability.AdvancedMetrics.Enums.SustainabilityTargetType>(s, true, out _))
            .WithMessage("TargetType must be one of: CarbonIntensityPerUnitOutput, EnergyIntensity, WaterIntensity, WasteDiversionRate, AbsoluteEmissionsReduction.");
        RuleFor(x => x.TargetDateUtc).GreaterThan(_ => DateTime.UtcNow);
    }
}
