// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           ClassifyMaterialCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ClassifyMaterialCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Validation;

/// <summary>Validates <see cref="Commands.ClassifyMaterialCommand"/>.</summary>
public sealed class ClassifyMaterialCommandValidator : AbstractValidator<Commands.ClassifyMaterialCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ClassifyMaterialCommand"/>.</summary>
    public ClassifyMaterialCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ClassifiedByUserId).NotEmpty();
        RuleFor(x => x.MaterialName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CasNumber).MaximumLength(20).When(x => x.CasNumber is not null);
        RuleFor(x => x.WasteProfileCode).MaximumLength(50).When(x => x.WasteProfileCode is not null);
        RuleFor(x => x.SdsHazardStatement).MaximumLength(2000).When(x => x.SdsHazardStatement is not null);
    }
}
