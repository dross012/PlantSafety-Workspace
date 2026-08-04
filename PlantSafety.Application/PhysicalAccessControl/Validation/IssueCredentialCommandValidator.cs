// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for IssueCredentialCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.IssueCredentialCommand"/>.</summary>
public sealed class IssueCredentialCommandValidator : AbstractValidator<Commands.IssueCredentialCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.IssueCredentialCommand"/>.</summary>
    public IssueCredentialCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.BadgeNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HolderUserId).NotEmpty();
        RuleFor(x => x.HolderType).NotEmpty();
        RuleFor(x => x.AccessLevel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
