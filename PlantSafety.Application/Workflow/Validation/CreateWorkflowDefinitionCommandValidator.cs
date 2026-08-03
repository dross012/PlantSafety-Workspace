// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           CreateWorkflowDefinitionCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for CreateWorkflowDefinitionCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3).
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;
using PlantSafety.Domain.Workflow.Enums;

namespace PlantSafety.Application.Workflow.Validation;

/// <summary>Validates <see cref="Commands.CreateWorkflowDefinitionCommand"/>.</summary>
public sealed class CreateWorkflowDefinitionCommandValidator : AbstractValidator<Commands.CreateWorkflowDefinitionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateWorkflowDefinitionCommand"/>.</summary>
    public CreateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.WorkflowTypeCode).NotEmpty().Must(WorkflowType.IsValid)
            .WithMessage("WorkflowTypeCode must be one of: Approval, Escalation, Notification, MultiStepProcess.");
    }
}
