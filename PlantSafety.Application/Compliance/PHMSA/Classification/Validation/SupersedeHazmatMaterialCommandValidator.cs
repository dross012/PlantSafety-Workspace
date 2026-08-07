// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           SupersedeHazmatMaterialCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for SupersedeHazmatMaterialCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Validation;

/// <summary>Validates <see cref="Commands.SupersedeHazmatMaterialCommand"/>.</summary>
public sealed class SupersedeHazmatMaterialCommandValidator : AbstractValidator<Commands.SupersedeHazmatMaterialCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SupersedeHazmatMaterialCommand"/>.</summary>
    public SupersedeHazmatMaterialCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazmatMaterialId).NotEmpty();
        RuleFor(x => x.SupersededByMaterialId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
