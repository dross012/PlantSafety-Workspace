// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for CompleteTransportTripCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.CompleteTransportTripCommand"/>.</summary>
public sealed class CompleteTransportTripCommandValidator : AbstractValidator<Commands.CompleteTransportTripCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CompleteTransportTripCommand"/>.</summary>
    public CompleteTransportTripCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransportTripId).NotEmpty();
        RuleFor(x => x.ActualArrivalAtUtc).GreaterThanOrEqualTo(x => x.ActualDepartureAtUtc);
        RuleFor(x => x.DistanceMiles).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
