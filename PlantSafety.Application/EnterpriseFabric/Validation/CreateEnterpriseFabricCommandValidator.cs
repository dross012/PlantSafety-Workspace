// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           CreateEnterpriseFabricCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateEnterpriseFabricCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Validation
//                  .CreateAuditingCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.EnterpriseFabric.Validation;

/// <summary>Validates <see cref="Commands.CreateEnterpriseFabricCommand"/>.</summary>
public sealed class CreateEnterpriseFabricCommandValidator : AbstractValidator<Commands.CreateEnterpriseFabricCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateEnterpriseFabricCommand"/>.</summary>
    public CreateEnterpriseFabricCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.FabricArea).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SystemReference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IntegrationType).NotEmpty().MaximumLength(100);
    }
}
