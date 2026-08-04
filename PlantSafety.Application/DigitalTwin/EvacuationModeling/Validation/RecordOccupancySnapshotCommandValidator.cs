// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordOccupancySnapshotCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;

/// <summary>Validates <see cref="Commands.RecordOccupancySnapshotCommand"/>.</summary>
public sealed class RecordOccupancySnapshotCommandValidator : AbstractValidator<Commands.RecordOccupancySnapshotCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordOccupancySnapshotCommand"/>.</summary>
    public RecordOccupancySnapshotCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EvacuationZoneId).NotEmpty();
        RuleFor(x => x.HeadcountEstimate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Source).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
