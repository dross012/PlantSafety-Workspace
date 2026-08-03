// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           ApproveStepCommandValidator.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ApproveStepCommand and RejectStepCommand.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Workflow.Validation;

/// <summary>Validates <see cref="Commands.ApproveStepCommand"/>.</summary>
public sealed class ApproveStepCommandValidator : AbstractValidator<Commands.ApproveStepCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ApproveStepCommand"/>.</summary>
    public ApproveStepCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.WorkflowInstanceId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

/// <summary>Validates <see cref="Commands.RejectStepCommand"/>.</summary>
public sealed class RejectStepCommandValidator : AbstractValidator<Commands.RejectStepCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RejectStepCommand"/>.</summary>
    public RejectStepCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RejectedByUserId).NotEmpty();
        RuleFor(x => x.WorkflowInstanceId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(2000);
    }
}
