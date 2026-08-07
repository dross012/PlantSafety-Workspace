// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           CreateIncidentReportCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateIncidentReportCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Validation;

/// <summary>Validates <see cref="Commands.CreateIncidentReportCommand"/>.</summary>
public sealed class CreateIncidentReportCommandValidator : AbstractValidator<Commands.CreateIncidentReportCommand>
{
    /// <summary>Small forward-clock tolerance to absorb minor client/server clock skew.</summary>
    private static readonly TimeSpan FutureDateTolerance = TimeSpan.FromMinutes(5);

    /// <summary>Defines validation rules for <see cref="Commands.CreateIncidentReportCommand"/>.</summary>
    public CreateIncidentReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ReportedByUserId).NotEmpty();
        RuleFor(x => x.IncidentType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.MaterialInvolvedDescription).MaximumLength(2000);
        RuleFor(x => x.Injuries).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PropertyDamageEstimateUsd).GreaterThanOrEqualTo(0m).When(x => x.PropertyDamageEstimateUsd.HasValue);
        RuleFor(x => x.Narrative).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.IncidentDateUtc)
            .Must(incidentDateUtc => incidentDateUtc <= DateTime.UtcNow.Add(FutureDateTolerance))
            .WithMessage("IncidentDateUtc must not be in the future.");
    }
}
