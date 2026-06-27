// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for UpdateObservationCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Update;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.Observation;

/// <summary>Validates <see cref="UpdateObservationCommand"/> structural fields.</summary>
public sealed class UpdateObservationCommandValidator : AbstractValidator<UpdateObservationCommand>
{
    private static readonly string[] ValidObservationTypes = ["Positive", "AtRisk", "Unsafe"];

    /// <summary>Defines validation rules.</summary>
    public UpdateObservationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ObservationId).NotEmpty();
        RuleFor(x => x.ETag).NotEmpty().WithMessage("ETag is required for concurrency control.");

        RuleFor(x => x).Must(x =>
            x.Title is not null ||
            x.Description is not null ||
            x.ObservationType is not null)
            .WithMessage("At least one field must be provided for update.");

        When(x => x.Title is not null, () =>
            RuleFor(x => x.Title!).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength));

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description!).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength));

        When(x => x.ObservationType is not null, () =>
            RuleFor(x => x.ObservationType!).Must(t => ValidObservationTypes.Contains(t))
                .WithMessage($"ObservationType must be one of: {string.Join(", ", ValidObservationTypes)}"));
    }
}
