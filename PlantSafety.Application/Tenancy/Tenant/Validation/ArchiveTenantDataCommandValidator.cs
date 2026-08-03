// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: FluentValidation validator for ArchiveTenantDataCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Tenancy.Tenant.Validation;

/// <summary>Validates <see cref="Commands.ArchiveTenantDataCommand"/>.</summary>
public sealed class ArchiveTenantDataCommandValidator : AbstractValidator<Commands.ArchiveTenantDataCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ArchiveTenantDataCommand"/>.</summary>
    public ArchiveTenantDataCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TargetTenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
