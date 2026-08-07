// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           SupersedePackagingCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for SupersedePackagingCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Validation;

/// <summary>Validates <see cref="Commands.SupersedePackagingCommand"/>.</summary>
public sealed class SupersedePackagingCommandValidator : AbstractValidator<Commands.SupersedePackagingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SupersedePackagingCommand"/>.</summary>
    public SupersedePackagingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PackagingSpecificationId).NotEmpty();
        RuleFor(x => x.SupersededBySpecificationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
