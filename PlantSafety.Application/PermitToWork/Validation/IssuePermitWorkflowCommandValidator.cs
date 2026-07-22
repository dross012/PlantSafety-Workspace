// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for IssuePermitWorkflowCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.IssuePermitWorkflowCommand"/>.</summary>
public sealed class IssuePermitWorkflowCommandValidator : AbstractValidator<Commands.IssuePermitWorkflowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.IssuePermitWorkflowCommand"/>.</summary>
    public IssuePermitWorkflowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitWorkflowId).NotEmpty();
        RuleFor(x => x.IssuerUserId).NotEmpty();
        RuleFor(x => x.ValidityHours).GreaterThan(0).When(x => x.ValidityHours.HasValue);
    }
}
