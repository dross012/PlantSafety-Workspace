// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: FluentValidation validator for CreateSafetyProjectCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.PlanningProjects.Commands.Projects.CreateSafetyProject;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.PlanningProjects.Validators.Projects;

/// <summary>Validates <see cref="CreateSafetyProjectCommand"/> structural fields.</summary>
public sealed class CreateSafetyProjectCommandValidator : AbstractValidator<CreateSafetyProjectCommand>
{
    /// <summary>Defines validation rules.</summary>
    public CreateSafetyProjectCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();

        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CommonValidationRules.MaxDescriptionLength);
        RuleFor(x => x.Priority)
            .Must(p => CommonValidationRules.ValidPriorities.Contains(p))
            .WithMessage($"Priority must be one of: {string.Join(", ", CommonValidationRules.ValidPriorities)}");
        RuleFor(x => x.ProjectOwnerId).NotEmpty();

        RuleFor(x => x.PlannedStartUtc).MustBeUtc("PlannedStartUtc");
        RuleFor(x => x.PlannedEndUtc).MustBeUtc("PlannedEndUtc");

        RuleFor(x => x).Must(x => x.PlannedStartUtc < x.PlannedEndUtc)
            .WithMessage("PlannedStartUtc must be before PlannedEndUtc.");
    }
}
