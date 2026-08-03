// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: FluentValidation validator for SuspendTenantCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Tenancy.Tenant.Validation;

/// <summary>Validates <see cref="Commands.SuspendTenantCommand"/>.</summary>
public sealed class SuspendTenantCommandValidator : AbstractValidator<Commands.SuspendTenantCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SuspendTenantCommand"/>.</summary>
    public SuspendTenantCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TargetTenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}
