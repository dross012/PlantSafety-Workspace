// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleFollowUpActiveMonitoringAuditingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleFollowUpActiveMonitoringAuditingCommand"/>.</summary>
public sealed class ScheduleFollowUpActiveMonitoringAuditingCommandValidator : AbstractValidator<Commands.ScheduleFollowUpActiveMonitoringAuditingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleFollowUpActiveMonitoringAuditingCommand"/>.</summary>
    public ScheduleFollowUpActiveMonitoringAuditingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActiveMonitoringAuditingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}