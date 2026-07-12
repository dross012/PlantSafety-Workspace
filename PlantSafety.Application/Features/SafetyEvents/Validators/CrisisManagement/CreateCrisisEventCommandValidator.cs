// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateCrisisEventCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.SafetyEvents.Commands.CrisisManagement.Create;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.SafetyEvents.Validators.CrisisManagement;

/// <summary>Validates <see cref="CreateCrisisEventCommand"/> structural fields.</summary>
public sealed class CreateCrisisEventCommandValidator : AbstractValidator<CreateCrisisEventCommand>
{
    private static readonly string[] ValidCrisisCategories =
        ["Fire", "Explosion", "ChemicalSpill", "MassInjury", "StructuralCollapse", "NaturalDisaster", "SecurityThreat", "Other"];

    /// <summary>Defines validation rules.</summary>
    public CreateCrisisEventCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.IncidentCommanderId).NotEmpty();

        RuleFor(x => x.CrisisCategory)
            .Must(c => ValidCrisisCategories.Contains(c))
            .WithMessage($"CrisisCategory must be one of: {string.Join(", ", ValidCrisisCategories)}");
    }
}
