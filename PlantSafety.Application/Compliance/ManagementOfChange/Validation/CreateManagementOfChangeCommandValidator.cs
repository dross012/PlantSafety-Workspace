// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           CreateManagementOfChangeCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateManagementOfChangeCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Validation;

/// <summary>Validates <see cref="Commands.CreateManagementOfChangeCommand"/>.</summary>
public sealed class CreateManagementOfChangeCommandValidator : AbstractValidator<Commands.CreateManagementOfChangeCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateManagementOfChangeCommand"/>.</summary>
    public CreateManagementOfChangeCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.ChangeTitle).NotEmpty().MaximumLength(400);
        RuleFor(x => x.ChangeType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Justification).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RiskLevel).NotEmpty().MaximumLength(50);
    }
}
