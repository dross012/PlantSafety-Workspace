// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for IssuePermitToWorkCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.IssuePermitToWorkCommand"/>.</summary>
public sealed class IssuePermitToWorkCommandValidator : AbstractValidator<Commands.IssuePermitToWorkCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.IssuePermitToWorkCommand"/>.</summary>
    public IssuePermitToWorkCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitToWorkId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.AuthorizedByUserId).NotEmpty();
        RuleFor(x => x.ValidityHours).GreaterThanOrEqualTo(0);
    }
}