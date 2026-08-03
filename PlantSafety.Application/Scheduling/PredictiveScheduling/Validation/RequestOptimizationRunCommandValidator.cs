// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Application
//  File:           RequestOptimizationRunCommandValidator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RequestOptimizationRunCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Validation;

/// <summary>Validates <see cref="Commands.RequestOptimizationRunCommand"/>.</summary>
public sealed class RequestOptimizationRunCommandValidator : AbstractValidator<Commands.RequestOptimizationRunCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RequestOptimizationRunCommand"/>.</summary>
    public RequestOptimizationRunCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Tasks).NotNull();
        RuleForEach(x => x.Tasks).ChildRules(task =>
        {
            task.RuleFor(t => t.Description).NotEmpty().MaximumLength(400);
            task.RuleFor(t => t.EstimatedDurationMinutes).GreaterThan(0);
            task.RuleFor(t => t.LatestFinishUtc).GreaterThan(t => t.EarliestStartUtc);
        });
        RuleFor(x => x.Availability).NotNull();
        RuleForEach(x => x.Availability).ChildRules(window =>
        {
            window.RuleFor(w => w.ResourceId).NotEmpty();
            window.RuleFor(w => w.EndUtc).GreaterThan(w => w.StartUtc);
        });
        RuleFor(x => x.ObjectiveCode)
            .NotEmpty()
            .Must(code => Enum.TryParse<OptimizationObjective>(code, ignoreCase: true, out _))
            .WithMessage("ObjectiveCode must be a recognized OptimizationObjective value.");
    }
}
