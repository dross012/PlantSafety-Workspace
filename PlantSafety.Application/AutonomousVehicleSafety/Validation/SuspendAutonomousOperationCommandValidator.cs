// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for SuspendAutonomousOperationCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.AutonomousVehicleSafety.Validation;

/// <summary>Validates <see cref="Commands.SuspendAutonomousOperationCommand"/>.</summary>
public sealed class SuspendAutonomousOperationCommandValidator : AbstractValidator<Commands.SuspendAutonomousOperationCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SuspendAutonomousOperationCommand"/>.</summary>
    public SuspendAutonomousOperationCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutonomousVehicleAssetId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
