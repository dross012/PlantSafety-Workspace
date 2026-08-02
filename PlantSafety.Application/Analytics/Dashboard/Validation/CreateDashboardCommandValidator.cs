// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           CreateDashboardCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateDashboardCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Analytics.Dashboard.Validation;

/// <summary>Validates <see cref="Commands.CreateDashboardCommand"/>.</summary>
public sealed class CreateDashboardCommandValidator : AbstractValidator<Commands.CreateDashboardCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateDashboardCommand"/>.</summary>
    public CreateDashboardCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.DashboardName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.DashboardType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RefreshIntervalSeconds).GreaterThanOrEqualTo(1);
        RuleFor(x => x.WidgetCount).GreaterThanOrEqualTo(0);
    }
}
