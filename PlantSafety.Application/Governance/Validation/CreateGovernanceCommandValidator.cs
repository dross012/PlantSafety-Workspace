// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           CreateGovernanceCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateGovernanceCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Governance.Validation;

/// <summary>Validates <see cref="Commands.CreateGovernanceCommand"/>.</summary>
public sealed class CreateGovernanceCommandValidator : AbstractValidator<Commands.CreateGovernanceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateGovernanceCommand"/>.</summary>
    public CreateGovernanceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.GovernanceArea).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReviewCycleMonths).GreaterThan(0);
    }
}
