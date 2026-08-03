// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           ReviseRouteDefinitionCommandValidator.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ReviseRouteDefinitionCommand. Additive-only
//                  evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.ProcessManagement.Routing.Validation;

/// <summary>Validates <see cref="Commands.ReviseRouteDefinitionCommand"/>.</summary>
public sealed class ReviseRouteDefinitionCommandValidator : AbstractValidator<Commands.ReviseRouteDefinitionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ReviseRouteDefinitionCommand"/>.</summary>
    public ReviseRouteDefinitionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RouteDefinitionId).NotEmpty();
        RuleFor(x => x.RevisedByUserId).NotEmpty();
        RuleFor(x => x.Steps).NotEmpty().WithMessage("A route definition must have at least one step.");
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Name).NotEmpty().MaximumLength(200);
        });
    }
}
