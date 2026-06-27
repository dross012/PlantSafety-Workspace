// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateDrillCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.DrillsAndExercises.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.DrillsAndExercises;

/// <summary>Validates <see cref="CreateDrillCommand"/> structural fields.</summary>
public sealed class CreateDrillCommandValidator : AbstractValidator<CreateDrillCommand>
{
    private static readonly string[] ValidDrillTypes =
        ["EvacuationDrill", "FireDrill", "MedicalEmergency", "HazMatResponse", "ActiveThreat", "TableTop", "FullScale"];

    /// <summary>Defines validation rules.</summary>
    public CreateDrillCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId<CreateDrillCommand, Guid>(x => x.CommandId);
        RuleFor(x => x.CorrelationId).IsValidId<CreateDrillCommand, Guid>(x => x.CorrelationId);
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);

        RuleFor(x => x.DrillType)
            .Must(t => ValidDrillTypes.Contains(t))
            .WithMessage($"DrillType must be one of: {string.Join(", ", ValidDrillTypes)}");

        RuleFor(x => x.ScheduledAtUtc).MustBeUtc();
    }
}
