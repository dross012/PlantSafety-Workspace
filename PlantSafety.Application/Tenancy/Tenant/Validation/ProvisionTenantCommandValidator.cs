// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: FluentValidation validator for ProvisionTenantCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Domain.Tenancy.Tenant.Policies;

namespace PlantSafety.Application.Tenancy.Tenant.Validation;

/// <summary>Validates <see cref="Commands.ProvisionTenantCommand"/>.</summary>
public sealed class ProvisionTenantCommandValidator : AbstractValidator<Commands.ProvisionTenantCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ProvisionTenantCommand"/>.</summary>
    public ProvisionTenantCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(TenantProvisioningPolicy.MaxNameLength);
        RuleFor(x => x.Region).NotEmpty().Must(TenantProvisioningPolicy.IsSupportedRegion)
            .WithMessage($"Region must be one of: {string.Join(", ", TenantProvisioningPolicy.SupportedRegions)}.");
        RuleFor(x => x.PlanCode).NotEmpty();
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminLastName).NotEmpty().MaximumLength(100);
    }
}
