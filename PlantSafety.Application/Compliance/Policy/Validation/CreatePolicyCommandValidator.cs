// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           CreatePolicyCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreatePolicyCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.Policy.Validation;

/// <summary>Validates <see cref="Commands.CreatePolicyCommand"/>.</summary>
public sealed class CreatePolicyCommandValidator : AbstractValidator<Commands.CreatePolicyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreatePolicyCommand"/>.</summary>
    public CreatePolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.PolicyTitle).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}
