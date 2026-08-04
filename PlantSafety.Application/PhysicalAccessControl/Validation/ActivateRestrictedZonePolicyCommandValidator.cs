// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for ActivateRestrictedZonePolicyCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PhysicalAccessControl.Validation;

/// <summary>Validates <see cref="Commands.ActivateRestrictedZonePolicyCommand"/>.</summary>
public sealed class ActivateRestrictedZonePolicyCommandValidator : AbstractValidator<Commands.ActivateRestrictedZonePolicyCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ActivateRestrictedZonePolicyCommand"/>.</summary>
    public ActivateRestrictedZonePolicyCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RestrictedZonePolicyId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
