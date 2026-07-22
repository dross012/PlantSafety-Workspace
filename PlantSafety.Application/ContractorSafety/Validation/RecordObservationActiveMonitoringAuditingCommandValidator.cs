// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordObservationActiveMonitoringAuditingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.ContractorSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordObservationActiveMonitoringAuditingCommand"/>.</summary>
public sealed class RecordObservationActiveMonitoringAuditingCommandValidator : AbstractValidator<Commands.RecordObservationActiveMonitoringAuditingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordObservationActiveMonitoringAuditingCommand"/>.</summary>
    public RecordObservationActiveMonitoringAuditingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActiveMonitoringAuditingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.UnsafeConditionCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.FindingNotes).MaximumLength(1000);
    }
}