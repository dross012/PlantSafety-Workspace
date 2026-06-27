// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for UpdateIncidentCommand.
//   At least one updatable field must be provided. ETag is required.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Update;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.IncidentManagement;

/// <summary>
/// Validates <see cref="UpdateIncidentCommand"/> before the handler executes.
/// </summary>
public sealed class UpdateIncidentCommandValidator : AbstractValidator<UpdateIncidentCommand>
{
    private static readonly string[] ValidSeverities = ["Minor", "Moderate", "Serious", "Critical"];

    /// <summary>Initialises all validation rules.</summary>
    public UpdateIncidentCommandValidator()
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

        RuleFor(x => x)
            .Must(HaveAtLeastOneChange)
            .WithMessage("At least one field must be provided for update.");

        When(x => x.Title is not null, () =>
            RuleFor(x => x.Title!)
                .NotEmpty().WithMessage("Title cannot be empty if provided.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters."));

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description!)
                .NotEmpty().WithMessage("Description cannot be empty if provided.")
                .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters."));

        When(x => x.Severity is not null, () =>
            RuleFor(x => x.Severity!)
                .Must(s => ValidSeverities.Contains(s, StringComparer.Ordinal))
                .WithMessage($"Severity must be one of: {string.Join(", ", ValidSeverities)}."));

        When(x => x.Location is not null, () =>
            RuleFor(x => x.Location!)
                .NotEmpty().WithMessage("Location cannot be empty if provided.")
                .MaximumLength(500).WithMessage("Location must not exceed 500 characters."));
    }

    private static bool HaveAtLeastOneChange(UpdateIncidentCommand cmd) =>
        cmd.Title is not null ||
        cmd.Description is not null ||
        cmd.Severity is not null ||
        cmd.Location is not null ||
        cmd.AssignedInvestigatorId.HasValue;
}
