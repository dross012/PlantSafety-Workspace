// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordLoadSecurementCheckCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordLoadSecurementCheckCommand"/>.</summary>
public sealed class RecordLoadSecurementCheckCommandValidator : AbstractValidator<Commands.RecordLoadSecurementCheckCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordLoadSecurementCheckCommand"/>.</summary>
    public RecordLoadSecurementCheckCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.CheckedByUserId).NotEmpty();
        RuleFor(x => x.DeficiencyCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SecurementScore).InclusiveBetween(0, 100);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
