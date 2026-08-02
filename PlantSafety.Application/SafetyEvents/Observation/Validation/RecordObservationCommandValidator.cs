// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Application
//  File:           RecordObservationCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecordObservationCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.Observation.Validation;

/// <summary>Validates <see cref="Commands.RecordObservationCommand"/>.</summary>
public sealed class RecordObservationCommandValidator : AbstractValidator<Commands.RecordObservationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordObservationCommand"/>.</summary>
    public RecordObservationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ObserverUserId).NotEmpty();
        RuleFor(x => x.ObservationType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.HazardCategory).MaximumLength(50);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
