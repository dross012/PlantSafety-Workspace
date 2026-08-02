// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           CreatePredictiveAnalyticsCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreatePredictiveAnalyticsCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Validation;

/// <summary>Validates <see cref="Commands.CreatePredictiveAnalyticsCommand"/>.</summary>
public sealed class CreatePredictiveAnalyticsCommandValidator : AbstractValidator<Commands.CreatePredictiveAnalyticsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreatePredictiveAnalyticsCommand"/>.</summary>
    public CreatePredictiveAnalyticsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PredictionType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PredictedRiskScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.ConfidenceLevel).InclusiveBetween(0m, 100m);
        RuleFor(x => x.PredictionHorizonDays).GreaterThanOrEqualTo(1);
    }
}
