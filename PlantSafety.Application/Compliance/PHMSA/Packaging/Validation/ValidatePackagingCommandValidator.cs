// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           ValidatePackagingCommandValidator.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ValidatePackagingCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Validation;

/// <summary>Validates <see cref="Commands.ValidatePackagingCommand"/>.</summary>
public sealed class ValidatePackagingCommandValidator : AbstractValidator<Commands.ValidatePackagingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ValidatePackagingCommand"/>.</summary>
    public ValidatePackagingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ProposedDOTSpec).NotEmpty().MaximumLength(20);
        RuleFor(x => x.HazardClass).InclusiveBetween(1, 9);
        RuleFor(x => x.PackingGroup).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
    }
}
