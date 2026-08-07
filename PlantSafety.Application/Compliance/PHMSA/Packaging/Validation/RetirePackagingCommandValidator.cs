// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           RetirePackagingCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RetirePackagingCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Validation;

/// <summary>Validates <see cref="Commands.RetirePackagingCommand"/>.</summary>
public sealed class RetirePackagingCommandValidator : AbstractValidator<Commands.RetirePackagingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetirePackagingCommand"/>.</summary>
    public RetirePackagingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PackagingSpecificationId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
