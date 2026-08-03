// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           AddWorkflowStepDefinitionCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for AddWorkflowStepDefinitionCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.ValueObjects;

namespace PlantSafety.Application.Workflow.Validation;

/// <summary>Validates <see cref="Commands.AddWorkflowStepDefinitionCommand"/>.</summary>
public sealed class AddWorkflowStepDefinitionCommandValidator : AbstractValidator<Commands.AddWorkflowStepDefinitionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AddWorkflowStepDefinitionCommand"/>.</summary>
    public AddWorkflowStepDefinitionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.WorkflowDefinitionId).NotEmpty();
        RuleFor(x => x.Order).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StepTypeCode).NotEmpty().Must(WorkflowStepType.IsValid)
            .WithMessage("StepTypeCode must be one of: HumanApproval, AutomatedDecision, PredictiveRecommendation, Notification, Escalation.");
        RuleFor(x => x.EntryConditionOperatorCode).Must(code => Enum.TryParse<GuardOperator>(code, ignoreCase: true, out _))
            .WithMessage("EntryConditionOperatorCode must be one of: Equals, NotEquals, GreaterThan, LessThan, Contains.");
        RuleFor(x => x.TimeoutSeconds).GreaterThan(0).When(x => x.TimeoutSeconds.HasValue);
        RuleFor(x => x.RetryMaxAttempts).GreaterThanOrEqualTo(0).When(x => x.RetryMaxAttempts.HasValue);
        RuleFor(x => x.RetryBaseDelaySeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.AssignedUserId.HasValue || !string.IsNullOrWhiteSpace(x.AssignedRoleCode) || !WorkflowStepType.From(x.StepTypeCode).RequiresHumanActor)
            .WithMessage("A human-actor step type requires either AssignedUserId or AssignedRoleCode.")
            .When(x => WorkflowStepType.IsValid(x.StepTypeCode));
    }
}
