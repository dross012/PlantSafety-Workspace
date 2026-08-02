// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           CreatePredictiveSafetyAlertCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreatePredictiveSafetyAlertCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Validation
//                  .CreateRCACommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Validation;

/// <summary>Validates <see cref="Commands.CreatePredictiveSafetyAlertCommand"/>.</summary>
public sealed class CreatePredictiveSafetyAlertCommandValidator : AbstractValidator<Commands.CreatePredictiveSafetyAlertCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreatePredictiveSafetyAlertCommand"/>.</summary>
    public CreatePredictiveSafetyAlertCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RiskCategory).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PredictedRiskScore).InclusiveBetween(0.0, 1.0);
        RuleFor(x => x.GeneratedByUserId).NotEmpty();
    }
}
