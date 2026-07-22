// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for SubmitForApprovalPpePoliciesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.SubmitForApprovalPpePoliciesCommand"/>.</summary>
public sealed class SubmitForApprovalPpePoliciesCommandValidator : AbstractValidator<Commands.SubmitForApprovalPpePoliciesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SubmitForApprovalPpePoliciesCommand"/>.</summary>
    public SubmitForApprovalPpePoliciesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PpePoliciesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}