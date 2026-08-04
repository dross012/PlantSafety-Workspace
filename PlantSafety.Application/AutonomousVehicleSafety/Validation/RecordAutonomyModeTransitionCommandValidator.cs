// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordAutonomyModeTransitionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.AutonomousVehicleSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordAutonomyModeTransitionCommand"/>.</summary>
public sealed class RecordAutonomyModeTransitionCommandValidator : AbstractValidator<Commands.RecordAutonomyModeTransitionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordAutonomyModeTransitionCommand"/>.</summary>
    public RecordAutonomyModeTransitionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutonomousVehicleAssetId).NotEmpty();
        RuleFor(x => x.ToMode).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
