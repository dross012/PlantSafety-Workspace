// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for SuspendCarrierCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.LogisticsTransportSafety.Validation;

/// <summary>Validates <see cref="Commands.SuspendCarrierCommand"/>.</summary>
public sealed class SuspendCarrierCommandValidator : AbstractValidator<Commands.SuspendCarrierCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SuspendCarrierCommand"/>.</summary>
    public SuspendCarrierCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CarrierId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
