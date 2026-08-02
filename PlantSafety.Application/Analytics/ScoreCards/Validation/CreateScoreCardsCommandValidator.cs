// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           CreateScoreCardsCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateScoreCardsCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Analytics.ScoreCards.Validation;

/// <summary>Validates <see cref="Commands.CreateScoreCardsCommand"/>.</summary>
public sealed class CreateScoreCardsCommandValidator : AbstractValidator<Commands.CreateScoreCardsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateScoreCardsCommand"/>.</summary>
    public CreateScoreCardsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.ScoreCardName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.ReportingPeriod).NotEmpty().MaximumLength(50);
        RuleFor(x => x.KpiCategory).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PerformanceIndex).InclusiveBetween(0m, 100m);
    }
}
