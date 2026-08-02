// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           CreateComplianceCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateComplianceCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.Compliance.Validation;

/// <summary>Validates <see cref="Commands.CreateComplianceCommand"/>.</summary>
public sealed class CreateComplianceCommandValidator : AbstractValidator<Commands.CreateComplianceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateComplianceCommand"/>.</summary>
    public CreateComplianceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ResponsiblePartyId).NotEmpty();
        RuleFor(x => x.RegulationReference).NotEmpty().MaximumLength(400);
        RuleFor(x => x.RuleDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.NonConformanceCount).GreaterThanOrEqualTo(0);
    }
}
