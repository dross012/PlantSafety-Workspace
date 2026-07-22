// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RejectPermitWorkflowCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.RejectPermitWorkflowCommand"/>.</summary>
public sealed class RejectPermitWorkflowCommandValidator : AbstractValidator<Commands.RejectPermitWorkflowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RejectPermitWorkflowCommand"/>.</summary>
    public RejectPermitWorkflowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitWorkflowId).NotEmpty();
        RuleFor(x => x.RejectedByUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
