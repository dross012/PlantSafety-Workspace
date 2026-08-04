// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for PrepareDischargeMonitoringReportCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.PrepareDischargeMonitoringReportCommand"/>.</summary>
public sealed class PrepareDischargeMonitoringReportCommandValidator : AbstractValidator<Commands.PrepareDischargeMonitoringReportCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.PrepareDischargeMonitoringReportCommand"/>.</summary>
    public PrepareDischargeMonitoringReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargePermitId).NotEmpty();
        RuleFor(x => x.ReportingPeriodEndUtc).GreaterThan(x => x.ReportingPeriodStartUtc);
        RuleFor(x => x.DueAtUtc).GreaterThanOrEqualTo(x => x.ReportingPeriodEndUtc);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}