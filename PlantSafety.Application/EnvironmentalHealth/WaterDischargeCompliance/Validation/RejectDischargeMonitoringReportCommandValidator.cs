// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RejectDischargeMonitoringReportCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.RejectDischargeMonitoringReportCommand"/>.</summary>
public sealed class RejectDischargeMonitoringReportCommandValidator : AbstractValidator<Commands.RejectDischargeMonitoringReportCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RejectDischargeMonitoringReportCommand"/>.</summary>
    public RejectDischargeMonitoringReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargeMonitoringReportId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}