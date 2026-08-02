// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           ScheduleDrillsAndExercisesCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ScheduleDrillsAndExercisesCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Validation;

/// <summary>Validates <see cref="Commands.ScheduleDrillsAndExercisesCommand"/>.</summary>
public sealed class ScheduleDrillsAndExercisesCommandValidator : AbstractValidator<Commands.ScheduleDrillsAndExercisesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleDrillsAndExercisesCommand"/>.</summary>
    public ScheduleDrillsAndExercisesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DrillType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ScheduledByUserId).NotEmpty();
    }
}
