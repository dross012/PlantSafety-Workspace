// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for PlanTransportTripCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.PlanTransportTripCommand"/>.</summary>
public sealed class PlanTransportTripCommandValidator : AbstractValidator<Commands.PlanTransportTripCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PlanTransportTripCommand"/>.</summary>
    public PlanTransportTripCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.DriverUserId).NotEmpty();
        RuleFor(x => x.TripType).NotEmpty();
        RuleFor(x => x.RouteDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PlannedArrivalAtUtc).GreaterThan(x => x.PlannedDepartureAtUtc);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
