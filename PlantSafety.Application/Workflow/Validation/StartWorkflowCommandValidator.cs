// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           StartWorkflowCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for StartWorkflowCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Workflow.Validation;

/// <summary>Validates <see cref="Commands.StartWorkflowCommand"/>.</summary>
public sealed class StartWorkflowCommandValidator : AbstractValidator<Commands.StartWorkflowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.StartWorkflowCommand"/>.</summary>
    public StartWorkflowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.WorkflowDefinitionId).NotEmpty();
    }
}
