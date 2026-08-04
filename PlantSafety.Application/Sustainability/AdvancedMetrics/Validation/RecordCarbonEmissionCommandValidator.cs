// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordCarbonEmissionCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecordCarbonEmissionCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Validation;

/// <summary>Validates <see cref="Commands.RecordCarbonEmissionCommand"/>.</summary>
public sealed class RecordCarbonEmissionCommandValidator : AbstractValidator<Commands.RecordCarbonEmissionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordCarbonEmissionCommand"/>.</summary>
    public RecordCarbonEmissionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ReportingPeriodEndUtc).GreaterThan(x => x.ReportingPeriodStartUtc);
        RuleFor(x => x.Scope).NotEmpty().Must(s => Enum.TryParse<Domain.Sustainability.AdvancedMetrics.Enums.GhgScope>(s, true, out _))
            .WithMessage("Scope must be one of: Scope1, Scope2, Scope3.");
        RuleFor(x => x.ActivityDescription).NotEmpty().MaximumLength(400);
        RuleFor(x => x.EmissionsMetricTonsCo2e).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MethodologyVersion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ActivityAtUtc).LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5));
    }
}
