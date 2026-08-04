// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GenerateCognitiveRiskPredictionCommandValidator.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for GenerateCognitiveRiskPredictionCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Validation;

/// <summary>Validates <see cref="Commands.GenerateCognitiveRiskPredictionCommand"/>.</summary>
public sealed class GenerateCognitiveRiskPredictionCommandValidator : AbstractValidator<Commands.GenerateCognitiveRiskPredictionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.GenerateCognitiveRiskPredictionCommand"/>.</summary>
    public GenerateCognitiveRiskPredictionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WorkerUserId).NotEmpty();
        RuleFor(x => x.SourceLedgerId).NotEmpty();
        RuleFor(x => x.ModelVersion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RequesterUserId).NotEmpty();
    }
}
