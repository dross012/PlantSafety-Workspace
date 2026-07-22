// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for BeginRepairEmergencyStopDevicesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.BeginRepairEmergencyStopDevicesCommand"/>.</summary>
public sealed class BeginRepairEmergencyStopDevicesCommandValidator : AbstractValidator<Commands.BeginRepairEmergencyStopDevicesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginRepairEmergencyStopDevicesCommand"/>.</summary>
    public BeginRepairEmergencyStopDevicesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyStopDevicesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}