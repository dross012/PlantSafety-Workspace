// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordEnergyIntensityMetricCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecordEnergyIntensityMetricCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Validation;

/// <summary>Validates <see cref="Commands.RecordEnergyIntensityMetricCommand"/>.</summary>
public sealed class RecordEnergyIntensityMetricCommandValidator : AbstractValidator<Commands.RecordEnergyIntensityMetricCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordEnergyIntensityMetricCommand"/>.</summary>
    public RecordEnergyIntensityMetricCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ReportingPeriodEndUtc).GreaterThan(x => x.ReportingPeriodStartUtc);
        RuleFor(x => x.EnergyConsumedKwh).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OutputUnits).GreaterThan(0);
        RuleFor(x => x.MethodologyVersion).NotEmpty().MaximumLength(100);
    }
}
