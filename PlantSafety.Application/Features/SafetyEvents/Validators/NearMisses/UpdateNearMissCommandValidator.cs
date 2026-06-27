// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for UpdateNearMissCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Update;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.NearMisses;

/// <summary>Validates <see cref="UpdateNearMissCommand"/> structural fields.</summary>
public sealed class UpdateNearMissCommandValidator : AbstractValidator<UpdateNearMissCommand>
{
    /// <summary>Defines validation rules.</summary>
    public UpdateNearMissCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.NearMissId).NotEmpty();
        RuleFor(x => x.ETag).NotEmpty().WithMessage("ETag is required for concurrency control.");

        RuleFor(x => x).Must(x =>
            x.Title is not null ||
            x.Description is not null ||
            x.PotentialSeverity is not null ||
            x.HazardCategory is not null)
            .WithMessage("At least one field must be provided for update.");

        When(x => x.Title is not null, () =>
            RuleFor(x => x.Title!).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength));

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description!).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength));

        When(x => x.PotentialSeverity is not null, () =>
            RuleFor(x => x.PotentialSeverity!).Must(s => CreateNearMissCommand.ValidSeverities.Contains(s))
                .WithMessage($"PotentialSeverity must be one of: {string.Join(", ", CreateNearMissCommand.ValidSeverities)}"));

        When(x => x.HazardCategory is not null, () =>
            RuleFor(x => x.HazardCategory!).Must(c => CreateNearMissCommand.ValidHazardCategories.Contains(c))
                .WithMessage($"HazardCategory must be one of: {string.Join(", ", CreateNearMissCommand.ValidHazardCategories)}"));
    }
}
