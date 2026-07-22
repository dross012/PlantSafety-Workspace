// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordMeasurementEnvironmentalHealthMonitoringCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordMeasurementEnvironmentalHealthMonitoringCommand"/>.</summary>
public sealed class RecordMeasurementEnvironmentalHealthMonitoringCommandValidator : AbstractValidator<Commands.RecordMeasurementEnvironmentalHealthMonitoringCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordMeasurementEnvironmentalHealthMonitoringCommand"/>.</summary>
    public RecordMeasurementEnvironmentalHealthMonitoringCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EnvironmentalHealthMonitoringId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ExposureLevelScore).InclusiveBetween(0m, 100m);
    }
}