// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for LinkSimulationToDrillCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;

/// <summary>Validates <see cref="Commands.LinkSimulationToDrillCommand"/>.</summary>
public sealed class LinkSimulationToDrillCommandValidator : AbstractValidator<Commands.LinkSimulationToDrillCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.LinkSimulationToDrillCommand"/>.</summary>
    public LinkSimulationToDrillCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EgressSimulationRunId).NotEmpty();
        RuleFor(x => x.DrillId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
