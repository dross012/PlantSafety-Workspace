// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validator for CreateNearMissCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.NearMisses;

/// <summary>Validates <see cref="CreateNearMissCommand"/>.</summary>
public sealed class CreateNearMissCommandValidator : AbstractValidator<CreateNearMissCommand>
{
    private static readonly string[] ValidSeverities = ["Minor", "Moderate", "Serious", "Critical"];
    private static readonly string[] ValidHazardCategories =
        ["Chemical", "Mechanical", "Electrical", "Ergonomic", "Biological", "Radiation", "Thermal", "Other"];

    /// <summary>Initialises all validation rules.</summary>
    public CreateNearMissCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(4000);

        RuleFor(x => x.OccurredAtUtc)
            .NotEqual(default(DateTimeOffset))
            .Must(d => d.Offset == TimeSpan.Zero).WithMessage("OccurredAtUtc must be UTC.")
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow).WithMessage("OccurredAtUtc cannot be in the future.");

        RuleFor(x => x.PotentialSeverity)
            .NotEmpty()
            .Must(s => ValidSeverities.Contains(s, StringComparer.Ordinal))
            .WithMessage($"PotentialSeverity must be one of: {string.Join(", ", ValidSeverities)}.");

        RuleFor(x => x.HazardCategory)
            .NotEmpty()
            .Must(c => ValidHazardCategories.Contains(c, StringComparer.Ordinal))
            .WithMessage($"HazardCategory must be one of: {string.Join(", ", ValidHazardCategories)}.");

        RuleFor(x => x.ReportedByWorkerId).NotEmpty();
    }
}
