// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           CreateAiSafetyIntelligenceCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateAiSafetyIntelligenceCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.AiSafetyIntelligence.Validation;

/// <summary>Validates <see cref="Commands.CreateAiSafetyIntelligenceCommand"/>.</summary>
public sealed class CreateAiSafetyIntelligenceCommandValidator : AbstractValidator<Commands.CreateAiSafetyIntelligenceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateAiSafetyIntelligenceCommand"/>.</summary>
    public CreateAiSafetyIntelligenceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.InsightType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ConfidenceScore).InclusiveBetween(0m, 1m);
        RuleFor(x => x.SourceModelName).NotEmpty().MaximumLength(200);
    }
}
