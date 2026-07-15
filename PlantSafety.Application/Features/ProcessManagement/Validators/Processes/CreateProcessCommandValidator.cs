// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: FluentValidation validator for CreateProcessCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.ProcessManagement.Commands.Processes.CreateProcess;
using PlantSafety.Application.Validation.Extensions;
using PlantSafety.Application.Validation.Rules;

namespace PlantSafety.Application.Features.ProcessManagement.Validators.Processes;

/// <summary>Validates <see cref="CreateProcessCommand"/> structural fields.</summary>
public sealed class CreateProcessCommandValidator : AbstractValidator<CreateProcessCommand>
{
    private static readonly string[] ValidCategories =
        ["HazardousMaterials", "ConfinedSpace", "LockoutTagout", "HotWork", "WorkAtHeight", "General"];

    /// <summary>Defines validation rules.</summary>
    public CreateProcessCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();

        RuleFor(x => x.ProcessName).NotEmpty().MaximumLength(CommonValidationRules.MaxTitleLength);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(CommonValidationRules.MaxReferenceLength);
    }
}
