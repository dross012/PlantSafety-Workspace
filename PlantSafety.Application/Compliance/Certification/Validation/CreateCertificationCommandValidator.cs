// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           CreateCertificationCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateCertificationCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.Certification.Validation;

/// <summary>Validates <see cref="Commands.CreateCertificationCommand"/>.</summary>
public sealed class CreateCertificationCommandValidator : AbstractValidator<Commands.CreateCertificationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateCertificationCommand"/>.</summary>
    public CreateCertificationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.HolderWorkerId).NotEmpty();
        RuleFor(x => x.CertificationName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.CertificationBody).NotEmpty().MaximumLength(400);
        RuleFor(x => x.CertificationNumber).NotEmpty().MaximumLength(200);
    }
}
