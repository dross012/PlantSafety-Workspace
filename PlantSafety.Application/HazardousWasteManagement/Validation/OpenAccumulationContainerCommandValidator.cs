// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for OpenAccumulationContainerCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.OpenAccumulationContainerCommand"/>.</summary>
public sealed class OpenAccumulationContainerCommandValidator : AbstractValidator<Commands.OpenAccumulationContainerCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.OpenAccumulationContainerCommand"/>.</summary>
    public OpenAccumulationContainerCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AccumulationAreaId).NotEmpty();
        RuleFor(x => x.WasteStreamId).NotEmpty();
        RuleFor(x => x.ContainerLabel).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CapacityGallons).GreaterThan(0);
        RuleFor(x => x.RegulatoryTimeLimitDays).GreaterThan(0);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
