// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireEmergencyStopDevicesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RetireEmergencyStopDevicesCommand"/>.</summary>
public sealed class RetireEmergencyStopDevicesCommandValidator : AbstractValidator<Commands.RetireEmergencyStopDevicesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireEmergencyStopDevicesCommand"/>.</summary>
    public RetireEmergencyStopDevicesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyStopDevicesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}