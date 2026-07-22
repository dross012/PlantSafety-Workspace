// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ResolveActiveMonitoringAuditingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.ResolveActiveMonitoringAuditingCommand"/>.</summary>
public sealed class ResolveActiveMonitoringAuditingCommandValidator : AbstractValidator<Commands.ResolveActiveMonitoringAuditingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ResolveActiveMonitoringAuditingCommand"/>.</summary>
    public ResolveActiveMonitoringAuditingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActiveMonitoringAuditingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}