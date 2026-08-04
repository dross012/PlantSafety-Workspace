// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for CloseAccumulationContainerCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.CloseAccumulationContainerCommand"/>.</summary>
public sealed class CloseAccumulationContainerCommandValidator : AbstractValidator<Commands.CloseAccumulationContainerCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CloseAccumulationContainerCommand"/>.</summary>
    public CloseAccumulationContainerCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WasteContainerId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
