// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           RecalculateWorkloadTrendCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecalculateWorkloadTrendCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

/// <summary>Validates <see cref="Commands.RecalculateWorkloadTrendCommand"/>.</summary>
public sealed class RecalculateWorkloadTrendCommandValidator : AbstractValidator<Commands.RecalculateWorkloadTrendCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecalculateWorkloadTrendCommand"/>.</summary>
    public RecalculateWorkloadTrendCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CognitiveLoadLedgerId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.PeriodEndUtc).GreaterThan(x => x.PeriodStartUtc);
    }
}
