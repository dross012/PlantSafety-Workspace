// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateObservationCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.Observation.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.Observation;

/// <summary>Validates <see cref="CreateObservationCommand"/> structural fields.</summary>
public sealed class CreateObservationCommandValidator : AbstractValidator<CreateObservationCommand>
{
    private static readonly string[] ValidObservationTypes = ["Positive", "AtRisk", "Unsafe"];

    /// <summary>Defines validation rules.</summary>
    public CreateObservationCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<CreateObservationCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.CorrelationId).IsValidId<CreateObservationCommand, Guid>(x => x.CorrelationId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.ObservedByWorkerId).NotEmpty();

        RuleFor(x => x.ObservationType)
            .Must(t => ValidObservationTypes.Contains(t))
            .WithMessage($"ObservationType must be one of: {string.Join(", ", ValidObservationTypes)}");

        RuleFor(x => x.ObservedAtUtc)
            .MustBeUtc()
            .MustNotBeInFuture();
    }
}
