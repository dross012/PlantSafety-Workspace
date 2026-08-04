// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for DefineRestrictedZonePolicyCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.DefineRestrictedZonePolicyCommand"/>.</summary>
public sealed class DefineRestrictedZonePolicyCommandValidator : AbstractValidator<Commands.DefineRestrictedZonePolicyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DefineRestrictedZonePolicyCommand"/>.</summary>
    public DefineRestrictedZonePolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyZoneId).NotEmpty();
        RuleFor(x => x.PolicyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AllowedAccessLevel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
