// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           CreateEngagementCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateEngagementCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Analytics.Engagement.Validation;

/// <summary>Validates <see cref="Commands.CreateEngagementCommand"/>.</summary>
public sealed class CreateEngagementCommandValidator : AbstractValidator<Commands.CreateEngagementCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateEngagementCommand"/>.</summary>
    public CreateEngagementCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WorkerId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.EngagementScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.SurveyPeriod).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ParticipationChannel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ResponseCount).GreaterThanOrEqualTo(0);
    }
}
