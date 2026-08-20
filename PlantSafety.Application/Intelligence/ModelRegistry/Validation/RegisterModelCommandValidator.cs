// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           RegisterModelCommandValidator.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RegisterModelCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Validation
//                  .CreateDataLakeActivityCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Validation;

/// <summary>Validates <see cref="Commands.RegisterModelCommand"/>.</summary>
public sealed class RegisterModelCommandValidator : AbstractValidator<Commands.RegisterModelCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterModelCommand"/>.</summary>
    public RegisterModelCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ModelVersion).NotEmpty().Matches(@"^\d+\.\d+\.\d+$")
            .WithMessage("ModelVersion must be in 'MAJOR.MINOR.PATCH' format.");
        RuleFor(x => x.DatasetHash).NotEmpty().Length(64)
            .WithMessage("DatasetHash must be a 64-character SHA-256 hexadecimal hash.");
        RuleFor(x => x.TrainingAccuracyScore).InclusiveBetween(0.0, 1.0);
        RuleFor(x => x.RegisteredByUserId).NotEmpty();
    }
}
