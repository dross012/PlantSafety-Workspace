// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordHumanRobotInteractionEventCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.AutonomousVehicleSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordHumanRobotInteractionEventCommand"/>.</summary>
public sealed class RecordHumanRobotInteractionEventCommandValidator : AbstractValidator<Commands.RecordHumanRobotInteractionEventCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordHumanRobotInteractionEventCommand"/>.</summary>
    public RecordHumanRobotInteractionEventCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutonomousVehicleAssetId).NotEmpty();
        RuleFor(x => x.InteractionType).NotEmpty();
        RuleFor(x => x.SeverityLevel).NotEmpty();
        RuleFor(x => x.DistanceMeters).GreaterThanOrEqualTo(0).When(x => x.DistanceMeters.HasValue);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
