// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           DeprecateModelCommandValidator.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for DeprecateModelCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3).
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Validation;

/// <summary>Validates <see cref="Commands.DeprecateModelCommand"/>.</summary>
public sealed class DeprecateModelCommandValidator : AbstractValidator<Commands.DeprecateModelCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeprecateModelCommand"/>.</summary>
    public DeprecateModelCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ModelRegistryEntryId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.DeprecatedByUserId).NotEmpty();
    }
}
