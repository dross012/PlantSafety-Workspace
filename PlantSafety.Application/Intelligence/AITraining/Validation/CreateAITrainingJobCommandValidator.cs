// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Application
//  File:           CreateAITrainingJobCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateAITrainingJobCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.ContinuousImprovement.RCA.Validation
//                  .CreateRCACommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Intelligence.AITraining.Validation;

/// <summary>Validates <see cref="Commands.CreateAITrainingJobCommand"/>.</summary>
public sealed class CreateAITrainingJobCommandValidator : AbstractValidator<Commands.CreateAITrainingJobCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateAITrainingJobCommand"/>.</summary>
    public CreateAITrainingJobCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ModelVersion).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InitiatedByUserId).NotEmpty();
    }
}
