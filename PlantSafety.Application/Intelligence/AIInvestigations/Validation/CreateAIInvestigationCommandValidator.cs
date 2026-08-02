// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           CreateAIInvestigationCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateAIInvestigationCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Validation
//                  .CreateRootCauseAnalysisCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;
using PlantSafety.Application.Intelligence.AIInvestigations.Commands;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Validation;

/// <summary>Validates <see cref="CreateAIInvestigationCommand"/>.</summary>
public sealed class CreateAIInvestigationCommandValidator : AbstractValidator<CreateAIInvestigationCommand>
{
    /// <summary>Defines validation rules for <see cref="CreateAIInvestigationCommand"/>.</summary>
    public CreateAIInvestigationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.IncidentReference).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AiConfidenceScore).InclusiveBetween(0.0, 1.0);
        RuleFor(x => x.AssignedToUserId).NotEmpty();
    }
}
