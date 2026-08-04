// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordAutonomousVehicleIncidentCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.AutonomousVehicleSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordAutonomousVehicleIncidentCommand"/>.</summary>
public sealed class RecordAutonomousVehicleIncidentCommandValidator : AbstractValidator<Commands.RecordAutonomousVehicleIncidentCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordAutonomousVehicleIncidentCommand"/>.</summary>
    public RecordAutonomousVehicleIncidentCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AutonomousVehicleAssetId).NotEmpty();
        RuleFor(x => x.IncidentType).NotEmpty();
        RuleFor(x => x.SeverityLevel).NotEmpty();
        RuleFor(x => x.DescriptionSummary).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
