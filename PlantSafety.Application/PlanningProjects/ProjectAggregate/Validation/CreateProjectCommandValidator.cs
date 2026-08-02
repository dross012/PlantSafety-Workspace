// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.ProjectAggregate
//  Layer:          Application
//  File:           CreateProjectCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateProjectCommand. Enforces invariants and
//                  normalization boundaries only; never calls repositories or external services
//                  (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Validation
//                  .CreateRootCauseAnalysisCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;
using PlantSafety.Application.PlanningProjects.ProjectAggregate.Commands;

namespace PlantSafety.Application.PlanningProjects.ProjectAggregate.Validation;

/// <summary>Validates <see cref="CreateProjectCommand"/>.</summary>
public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    /// <summary>Defines validation rules for <see cref="CreateProjectCommand"/>.</summary>
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerUserId).NotEmpty();
    }
}
