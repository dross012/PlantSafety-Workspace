// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordInspectionResultTransportVehicleCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordInspectionResultTransportVehicleCommand"/>.</summary>
public sealed class RecordInspectionResultTransportVehicleCommandValidator : AbstractValidator<Commands.RecordInspectionResultTransportVehicleCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordInspectionResultTransportVehicleCommand"/>.</summary>
    public RecordInspectionResultTransportVehicleCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransportVehicleId).NotEmpty();
        RuleFor(x => x.InspectionScore).InclusiveBetween(0, 100);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
