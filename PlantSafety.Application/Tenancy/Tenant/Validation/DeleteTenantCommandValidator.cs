// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: FluentValidation validator for DeleteTenantCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Tenancy.Tenant.Validation;

/// <summary>Validates <see cref="Commands.DeleteTenantCommand"/>.</summary>
public sealed class DeleteTenantCommandValidator : AbstractValidator<Commands.DeleteTenantCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.DeleteTenantCommand"/>.</summary>
    public DeleteTenantCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TargetTenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}
