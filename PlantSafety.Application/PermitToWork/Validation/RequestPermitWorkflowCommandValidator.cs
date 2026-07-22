// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RequestPermitWorkflowCommand. Enforces
// invariants and normalization boundaries only; never calls repositories or external services
// (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.RequestPermitWorkflowCommand"/>.</summary>
public sealed class RequestPermitWorkflowCommandValidator : AbstractValidator<Commands.RequestPermitWorkflowCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RequestPermitWorkflowCommand"/>.</summary>
    public RequestPermitWorkflowCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.PermitNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PermitTypeCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.WorkDescription).NotEmpty().MaximumLength(2000);
    }
}
