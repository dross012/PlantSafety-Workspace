// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for ScheduleInspectionTransportVehicleCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleInspectionTransportVehicleCommand"/>.</summary>
public sealed class ScheduleInspectionTransportVehicleCommandValidator : AbstractValidator<Commands.ScheduleInspectionTransportVehicleCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleInspectionTransportVehicleCommand"/>.</summary>
    public ScheduleInspectionTransportVehicleCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransportVehicleId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
