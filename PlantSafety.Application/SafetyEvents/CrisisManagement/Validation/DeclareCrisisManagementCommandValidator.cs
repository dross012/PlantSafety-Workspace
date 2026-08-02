// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           DeclareCrisisManagementCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for DeclareCrisisManagementCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Validation;

/// <summary>Validates <see cref="Commands.DeclareCrisisManagementCommand"/>.</summary>
public sealed class DeclareCrisisManagementCommandValidator : AbstractValidator<Commands.DeclareCrisisManagementCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeclareCrisisManagementCommand"/>.</summary>
    public DeclareCrisisManagementCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Severity).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DeclaredAtUtc).NotEmpty();
        RuleFor(x => x.DeclaredByUserId).NotEmpty();
    }
}
