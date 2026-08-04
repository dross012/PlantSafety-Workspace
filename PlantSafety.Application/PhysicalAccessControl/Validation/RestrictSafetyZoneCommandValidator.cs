// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RestrictSafetyZoneCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.RestrictSafetyZoneCommand"/>.</summary>
public sealed class RestrictSafetyZoneCommandValidator : AbstractValidator<Commands.RestrictSafetyZoneCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RestrictSafetyZoneCommand"/>.</summary>
    public RestrictSafetyZoneCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyZoneId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
