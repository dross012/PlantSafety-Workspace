// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterCarrierCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.RegisterCarrierCommand"/>.</summary>
public sealed class RegisterCarrierCommandValidator : AbstractValidator<Commands.RegisterCarrierCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterCarrierCommand"/>.</summary>
    public RegisterCarrierCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CarrierName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CarrierType).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
