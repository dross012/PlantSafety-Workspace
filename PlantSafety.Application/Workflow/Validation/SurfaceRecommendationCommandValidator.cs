// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           SurfaceRecommendationCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for SurfaceRecommendationCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Workflow.Validation;

/// <summary>Validates <see cref="Commands.SurfaceRecommendationCommand"/>.</summary>
public sealed class SurfaceRecommendationCommandValidator : AbstractValidator<Commands.SurfaceRecommendationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SurfaceRecommendationCommand"/>.</summary>
    public SurfaceRecommendationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.WorkflowInstanceId).NotEmpty();
        RuleFor(x => x.RecommendationReference).NotEmpty().MaximumLength(400);
    }
}
