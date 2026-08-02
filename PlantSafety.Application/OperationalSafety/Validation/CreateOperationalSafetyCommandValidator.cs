// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           CreateOperationalSafetyCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateOperationalSafetyCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.OperationalSafety.Validation;

/// <summary>Validates <see cref="Commands.CreateOperationalSafetyCommand"/>.</summary>
public sealed class CreateOperationalSafetyCommandValidator : AbstractValidator<Commands.CreateOperationalSafetyCommand>
{
    private static readonly string[] ValidRiskLevels = ["Low", "Medium", "High", "Critical"];

    /// <summary>Defines validation rules for <see cref="Commands.CreateOperationalSafetyCommand"/>.</summary>
    public CreateOperationalSafetyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.SafetyDomain).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RiskLevel).NotEmpty().Must(r => ValidRiskLevels.Contains(r))
            .WithMessage("RiskLevel must be one of: Low, Medium, High, Critical.");
    }
}
