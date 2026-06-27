// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for ApprovePermitCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Approve;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.Compliance.Validators.Permits;

/// <summary>Validates <see cref="ApprovePermitCommand"/> structural fields.</summary>
public sealed class ApprovePermitCommandValidator : AbstractValidator<ApprovePermitCommand>
{
    /// <summary>Defines validation rules.</summary>
    public ApprovePermitCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<ApprovePermitCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.PermitId).NotEmpty();
        RuleFor(x => x.ETag).NotEmpty().WithMessage("ETag is required for concurrency control.");

        When(x => x.ApprovalNotes is not null, () =>
            RuleFor(x => x.ApprovalNotes!).MaximumLength(CommonValidationRules.MaxNotesLength));
    }
}
