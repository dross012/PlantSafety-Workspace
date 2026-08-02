// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           CreateHumanFactorsCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateHumanFactorsCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.HumanFactors.Validation;

/// <summary>Validates <see cref="Commands.CreateHumanFactorsCommand"/>.</summary>
public sealed class CreateHumanFactorsCommandValidator : AbstractValidator<Commands.CreateHumanFactorsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateHumanFactorsCommand"/>.</summary>
    public CreateHumanFactorsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.FactorType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SeverityRating).InclusiveBetween(1, 5);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
