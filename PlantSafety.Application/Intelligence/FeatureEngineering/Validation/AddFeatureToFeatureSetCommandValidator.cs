// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           AddFeatureToFeatureSetCommandValidator.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for AddFeatureToFeatureSetCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3).
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Validation;

/// <summary>Validates <see cref="Commands.AddFeatureToFeatureSetCommand"/>.</summary>
public sealed class AddFeatureToFeatureSetCommandValidator : AbstractValidator<Commands.AddFeatureToFeatureSetCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AddFeatureToFeatureSetCommand"/>.</summary>
    public AddFeatureToFeatureSetCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.FeatureSetId).NotEmpty();
        RuleFor(x => x.FeatureName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RequestedByUserId).NotEmpty();
    }
}
