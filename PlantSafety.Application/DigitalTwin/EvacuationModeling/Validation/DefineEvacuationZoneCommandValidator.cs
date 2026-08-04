// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DefineEvacuationZoneCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;

/// <summary>Validates <see cref="Commands.DefineEvacuationZoneCommand"/>.</summary>
public sealed class DefineEvacuationZoneCommandValidator : AbstractValidator<Commands.DefineEvacuationZoneCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DefineEvacuationZoneCommand"/>.</summary>
    public DefineEvacuationZoneCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ZoneName).NotEmpty().MaximumLength(400);
        RuleFor(x => x.TwinModelVersion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZoneCapacity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OccupancyThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
