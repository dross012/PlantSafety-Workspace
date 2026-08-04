// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordTransporterPickupCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.HazardousWasteManagement.Validation;

/// <summary>Validates <see cref="Commands.RecordTransporterPickupCommand"/>.</summary>
public sealed class RecordTransporterPickupCommandValidator : AbstractValidator<Commands.RecordTransporterPickupCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordTransporterPickupCommand"/>.</summary>
    public RecordTransporterPickupCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.HazardousWasteManifestId).NotEmpty();
        RuleFor(x => x.TransporterId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
