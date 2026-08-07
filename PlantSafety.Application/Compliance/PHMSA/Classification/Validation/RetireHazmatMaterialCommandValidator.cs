// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           RetireHazmatMaterialCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RetireHazmatMaterialCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Validation;

/// <summary>Validates <see cref="Commands.RetireHazmatMaterialCommand"/>.</summary>
public sealed class RetireHazmatMaterialCommandValidator : AbstractValidator<Commands.RetireHazmatMaterialCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireHazmatMaterialCommand"/>.</summary>
    public RetireHazmatMaterialCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazmatMaterialId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
