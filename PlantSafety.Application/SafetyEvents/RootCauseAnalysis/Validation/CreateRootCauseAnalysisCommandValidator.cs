// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           CreateRootCauseAnalysisCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateRootCauseAnalysisCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Validation;

/// <summary>Validates <see cref="Commands.CreateRootCauseAnalysisCommand"/>.</summary>
public sealed class CreateRootCauseAnalysisCommandValidator : AbstractValidator<Commands.CreateRootCauseAnalysisCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateRootCauseAnalysisCommand"/>.</summary>
    public CreateRootCauseAnalysisCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Methodology).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LeadAnalystUserId).NotEmpty();
    }
}
