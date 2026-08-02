// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           CreateAiGovernanceCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateAiGovernanceCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation.CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.AiGovernance.Validation;

/// <summary>Validates <see cref="Commands.CreateAiGovernanceCommand"/>.</summary>
public sealed class CreateAiGovernanceCommandValidator : AbstractValidator<Commands.CreateAiGovernanceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateAiGovernanceCommand"/>.</summary>
    public CreateAiGovernanceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.SystemName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AiUseCase).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RiskClassification).NotEmpty().MaximumLength(50);
    }
}
