// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: FluentValidation validator for ExportTenantDataCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Tenancy.Tenant.Validation;

/// <summary>Validates <see cref="Commands.ExportTenantDataCommand"/>.</summary>
public sealed class ExportTenantDataCommandValidator : AbstractValidator<Commands.ExportTenantDataCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ExportTenantDataCommand"/>.</summary>
    public ExportTenantDataCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TargetTenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
