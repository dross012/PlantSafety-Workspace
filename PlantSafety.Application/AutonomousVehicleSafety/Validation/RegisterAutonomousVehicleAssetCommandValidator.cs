// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterAutonomousVehicleAssetCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.AutonomousVehicleSafety.Validation;

/// <summary>Validates <see cref="Commands.RegisterAutonomousVehicleAssetCommand"/>.</summary>
public sealed class RegisterAutonomousVehicleAssetCommandValidator : AbstractValidator<Commands.RegisterAutonomousVehicleAssetCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterAutonomousVehicleAssetCommand"/>.</summary>
    public RegisterAutonomousVehicleAssetCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AssetTag).NotEmpty().MaximumLength(200);
        RuleFor(x => x.VehicleType).NotEmpty();
        RuleFor(x => x.AutonomyLevel).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
