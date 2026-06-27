// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateIncidentCommand.
//   Rules: structural/format validation only. Never calls repos, logs, or mutates state.
//   Throws ValidationException via ValidationBehavior pipeline — never throw directly.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.IncidentManagement.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.IncidentManagement;

/// <summary>
/// Validates <see cref="CreateIncidentCommand"/> before the handler executes.
/// All failures are collected and thrown as a single <see cref="FluentValidation.ValidationException"/>.
/// </summary>
public sealed class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    private static readonly string[] ValidSeverities = ["Minor", "Moderate", "Serious", "Critical"];

    /// <summary>Initialises all validation rules.</summary>
    public CreateIncidentCommandValidator()
    {
        RuleFor(x => x.CommandId)
            .NotEmpty().WithMessage("CommandId is required.");

        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage("CorrelationId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.ActorId)
            .NotEmpty().WithMessage("ActorId is required.");

        RuleFor(x => x.SiteId)
            .NotEmpty().WithMessage("SiteId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");

        RuleFor(x => x.IncidentOccurredAtUtc)
            .NotEqual(default(DateTimeOffset)).WithMessage("IncidentOccurredAtUtc is required.")
            .Must(BeUtc).WithMessage("IncidentOccurredAtUtc must be UTC.")
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow).WithMessage("IncidentOccurredAtUtc cannot be in the future.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required.")
            .Must(s => ValidSeverities.Contains(s, StringComparer.Ordinal))
            .WithMessage($"Severity must be one of: {string.Join(", ", ValidSeverities)}.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters.");

        RuleFor(x => x.ReportedByWorkerId)
            .NotEmpty().WithMessage("ReportedByWorkerId is required.");
    }

    private static bool BeUtc(DateTimeOffset dt) => dt.Offset == TimeSpan.Zero;
}
