// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetirePresenceSensingDevicesCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RetirePresenceSensingDevicesCommand"/>.</summary>
public sealed class RetirePresenceSensingDevicesCommandValidator : AbstractValidator<Commands.RetirePresenceSensingDevicesCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetirePresenceSensingDevicesCommand"/>.</summary>
    public RetirePresenceSensingDevicesCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PresenceSensingDevicesId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}