// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterTransportVehicleCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.RegisterTransportVehicleCommand"/>.</summary>
public sealed class RegisterTransportVehicleCommandValidator : AbstractValidator<Commands.RegisterTransportVehicleCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterTransportVehicleCommand"/>.</summary>
    public RegisterTransportVehicleCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.VehicleTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.VehicleType).NotEmpty();
        RuleFor(x => x.VehicleIdentifier).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
