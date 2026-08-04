// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for CancelTransportTripCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.CancelTransportTripCommand"/>.</summary>
public sealed class CancelTransportTripCommandValidator : AbstractValidator<Commands.CancelTransportTripCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CancelTransportTripCommand"/>.</summary>
    public CancelTransportTripCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransportTripId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
