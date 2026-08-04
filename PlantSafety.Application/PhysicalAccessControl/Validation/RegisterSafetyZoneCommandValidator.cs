// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RegisterSafetyZoneCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.RegisterSafetyZoneCommand"/>.</summary>
public sealed class RegisterSafetyZoneCommandValidator : AbstractValidator<Commands.RegisterSafetyZoneCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RegisterSafetyZoneCommand"/>.</summary>
    public RegisterSafetyZoneCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ZoneCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZoneName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HazardLevel).NotEmpty();
        RuleFor(x => x.MaxOccupancy).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
