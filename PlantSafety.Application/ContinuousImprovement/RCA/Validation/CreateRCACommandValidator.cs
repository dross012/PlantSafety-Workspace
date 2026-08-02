// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           CreateRCACommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateRCACommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Validation
//                  .CreateRootCauseAnalysisCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Validation;

/// <summary>Validates <see cref="Commands.CreateRCACommand"/>.</summary>
public sealed class CreateRCACommandValidator : AbstractValidator<Commands.CreateRCACommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateRCACommand"/>.</summary>
    public CreateRCACommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ProblemStatement).NotEmpty();
        RuleFor(x => x.Methodology).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OwnerUserId).NotEmpty();
    }
}
