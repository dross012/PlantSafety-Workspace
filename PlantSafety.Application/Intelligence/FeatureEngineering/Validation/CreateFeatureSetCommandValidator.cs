// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           CreateFeatureSetCommandValidator.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateFeatureSetCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Validation
//                  .CreateDataLakeActivityCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Validation;

/// <summary>Validates <see cref="Commands.CreateFeatureSetCommand"/>.</summary>
public sealed class CreateFeatureSetCommandValidator : AbstractValidator<Commands.CreateFeatureSetCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateFeatureSetCommand"/>.</summary>
    public CreateFeatureSetCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Version).Matches(@"^\d+\.\d+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Version))
            .WithMessage("Version must be in 'MAJOR.MINOR' format.");
        RuleFor(x => x.DataSourcePath).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
    }
}
