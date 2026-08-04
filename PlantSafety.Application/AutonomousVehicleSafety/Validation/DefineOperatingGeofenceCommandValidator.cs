// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DefineOperatingGeofenceCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.AutonomousVehicleSafety.Validation;

/// <summary>Validates <see cref="Commands.DefineOperatingGeofenceCommand"/>.</summary>
public sealed class DefineOperatingGeofenceCommandValidator : AbstractValidator<Commands.DefineOperatingGeofenceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DefineOperatingGeofenceCommand"/>.</summary>
    public DefineOperatingGeofenceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.GeofenceName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ZoneType).NotEmpty();
        RuleFor(x => x.BoundaryDefinition).NotEmpty();
        RuleFor(x => x.MaxSpeedKph).GreaterThan(0).When(x => x.MaxSpeedKph.HasValue);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
