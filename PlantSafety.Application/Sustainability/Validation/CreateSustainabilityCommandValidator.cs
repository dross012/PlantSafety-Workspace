// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           CreateSustainabilityCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateSustainabilityCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Sustainability.Validation;

/// <summary>Validates <see cref="Commands.CreateSustainabilityCommand"/>.</summary>
public sealed class CreateSustainabilityCommandValidator : AbstractValidator<Commands.CreateSustainabilityCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateSustainabilityCommand"/>.</summary>
    public CreateSustainabilityCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.InitiativeName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetValue).GreaterThanOrEqualTo(0).When(x => x.TargetValue.HasValue);
    }
}
