// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for SubmitDischargeMonitoringReportCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Validation;

/// <summary>Validates <see cref="Commands.SubmitDischargeMonitoringReportCommand"/>.</summary>
public sealed class SubmitDischargeMonitoringReportCommandValidator : AbstractValidator<Commands.SubmitDischargeMonitoringReportCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SubmitDischargeMonitoringReportCommand"/>.</summary>
    public SubmitDischargeMonitoringReportCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.DischargeMonitoringReportId).NotEmpty();
        RuleFor(x => x.TotalSamplesCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SamplesWithinLimitCount).GreaterThanOrEqualTo(0).LessThanOrEqualTo(x => x.TotalSamplesCount);
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}