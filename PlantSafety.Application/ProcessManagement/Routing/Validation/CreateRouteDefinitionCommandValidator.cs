// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           CreateRouteDefinitionCommandValidator.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateRouteDefinitionCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.ProcessManagement.Validation.CreateProcessDefinitionCommandValidator,
//                  extended with step-list invariants (at least one step; each step named).
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.ProcessManagement.Routing.Validation;

/// <summary>Validates <see cref="Commands.CreateRouteDefinitionCommand"/>.</summary>
public sealed class CreateRouteDefinitionCommandValidator : AbstractValidator<Commands.CreateRouteDefinitionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateRouteDefinitionCommand"/>.</summary>
    public CreateRouteDefinitionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PlantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Steps).NotEmpty().WithMessage("A route definition must have at least one step.");
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Name).NotEmpty().MaximumLength(200);
        });
    }
}
