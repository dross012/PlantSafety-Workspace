// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordTransportIncidentCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordTransportIncidentCommand"/>.</summary>
public sealed class RecordTransportIncidentCommandValidator : AbstractValidator<Commands.RecordTransportIncidentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordTransportIncidentCommand"/>.</summary>
    public RecordTransportIncidentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.DriverUserId).NotEmpty();
        RuleFor(x => x.IncidentType).NotEmpty();
        RuleFor(x => x.Severity).NotEmpty();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
