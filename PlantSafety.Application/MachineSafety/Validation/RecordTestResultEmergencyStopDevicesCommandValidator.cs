// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordTestResultEmergencyStopDevicesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordTestResultEmergencyStopDevicesCommand"/>.</summary>
public sealed class RecordTestResultEmergencyStopDevicesCommandValidator : AbstractValidator<Commands.RecordTestResultEmergencyStopDevicesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordTestResultEmergencyStopDevicesCommand"/>.</summary>
    public RecordTestResultEmergencyStopDevicesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyStopDevicesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ResponseTimeScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.TestedByUserId).NotEmpty();
    }
}