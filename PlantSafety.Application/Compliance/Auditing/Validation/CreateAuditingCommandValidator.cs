// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           CreateAuditingCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateAuditingCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.Auditing.Validation;

/// <summary>Validates <see cref="Commands.CreateAuditingCommand"/>.</summary>
public sealed class CreateAuditingCommandValidator : AbstractValidator<Commands.CreateAuditingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateAuditingCommand"/>.</summary>
    public CreateAuditingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.AuditorId).NotEmpty();
        RuleFor(x => x.AuditTitle).NotEmpty().MaximumLength(400);
        RuleFor(x => x.AuditType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(2000);
    }
}
