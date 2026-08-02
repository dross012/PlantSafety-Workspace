// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           DeclareEmergencyCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for DeclareEmergencyCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Validation;

/// <summary>Validates <see cref="Commands.DeclareEmergencyCommand"/>.</summary>
public sealed class DeclareEmergencyCommandValidator : AbstractValidator<Commands.DeclareEmergencyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeclareEmergencyCommand"/>.</summary>
    public DeclareEmergencyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DeclaredByUserId).NotEmpty();
        RuleFor(x => x.EmergencyType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Severity).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(400);
        RuleFor(x => x.CasualtyCount).GreaterThanOrEqualTo(0);
    }
}
