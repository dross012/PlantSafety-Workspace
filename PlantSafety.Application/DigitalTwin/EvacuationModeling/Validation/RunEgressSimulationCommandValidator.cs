// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RunEgressSimulationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;

/// <summary>Validates <see cref="Commands.RunEgressSimulationCommand"/>.</summary>
public sealed class RunEgressSimulationCommandValidator : AbstractValidator<Commands.RunEgressSimulationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RunEgressSimulationCommand"/>.</summary>
    public RunEgressSimulationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ZoneId).NotEmpty();
        RuleFor(x => x.TwinModelVersion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TriggerReason).NotEmpty();
        RuleFor(x => x.SimulatedEvacuationTimeSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
