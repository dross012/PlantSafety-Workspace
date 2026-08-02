// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           CreateNearMissesCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateNearMissesCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Validation;

/// <summary>Validates <see cref="Commands.CreateNearMissesCommand"/>.</summary>
public sealed class CreateNearMissesCommandValidator : AbstractValidator<Commands.CreateNearMissesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateNearMissesCommand"/>.</summary>
    public CreateNearMissesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PotentialSeverity).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReportedAtUtc).NotEmpty();
        RuleFor(x => x.ReportedByUserId).NotEmpty();
    }
}
