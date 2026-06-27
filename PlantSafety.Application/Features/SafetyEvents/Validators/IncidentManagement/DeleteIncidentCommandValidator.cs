// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for DeleteIncidentCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Delete;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.IncidentManagement;

/// <summary>
/// Validates <see cref="DeleteIncidentCommand"/> before the handler executes.
/// </summary>
public sealed class DeleteIncidentCommandValidator : AbstractValidator<DeleteIncidentCommand>
{
    /// <summary>Initialises all validation rules.</summary>
    public DeleteIncidentCommandValidator()
    {
        RuleFor(x => x.CommandId)
            .NotEmpty().WithMessage("CommandId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.ActorId)
            .NotEmpty().WithMessage("ActorId is required.");

        RuleFor(x => x.IncidentId)
            .NotEmpty().WithMessage("IncidentId is required.");

        RuleFor(x => x.ETag)
            .NotEmpty().WithMessage("ETag is required for optimistic concurrency.");

        RuleFor(x => x.DeletionReason)
            .NotEmpty().WithMessage("DeletionReason is required.")
            .MaximumLength(1000).WithMessage("DeletionReason must not exceed 1000 characters.");
    }
}
