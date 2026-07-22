// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleTestEmergencyStopDevicesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleTestEmergencyStopDevicesCommand"/>.</summary>
public sealed class ScheduleTestEmergencyStopDevicesCommandValidator : AbstractValidator<Commands.ScheduleTestEmergencyStopDevicesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleTestEmergencyStopDevicesCommand"/>.</summary>
    public ScheduleTestEmergencyStopDevicesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyStopDevicesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}