// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           AuthorizePackagingCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for AuthorizePackagingCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Validation;

/// <summary>Validates <see cref="Commands.AuthorizePackagingCommand"/>.</summary>
public sealed class AuthorizePackagingCommandValidator : AbstractValidator<Commands.AuthorizePackagingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AuthorizePackagingCommand"/>.</summary>
    public AuthorizePackagingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AuthorizedByUserId).NotEmpty();
        RuleFor(x => x.DOTSpec).NotEmpty().MaximumLength(20);
        RuleFor(x => x.MaxCapacity).GreaterThan(0);
        RuleFor(x => x.CapacityUnit).NotEmpty().MaximumLength(10);
        RuleFor(x => x.ClosureInstructions).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ReuseRestrictions).MaximumLength(4000).When(x => x.ReuseRestrictions is not null);
    }
}
