// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RevokeCredentialCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.RevokeCredentialCommand"/>.</summary>
public sealed class RevokeCredentialCommandValidator : AbstractValidator<Commands.RevokeCredentialCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RevokeCredentialCommand"/>.</summary>
    public RevokeCredentialCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CredentialId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
