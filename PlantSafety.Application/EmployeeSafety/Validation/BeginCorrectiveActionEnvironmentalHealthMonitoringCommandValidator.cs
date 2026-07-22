// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for BeginCorrectiveActionEnvironmentalHealthMonitoringCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.BeginCorrectiveActionEnvironmentalHealthMonitoringCommand"/>.</summary>
public sealed class BeginCorrectiveActionEnvironmentalHealthMonitoringCommandValidator : AbstractValidator<Commands.BeginCorrectiveActionEnvironmentalHealthMonitoringCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginCorrectiveActionEnvironmentalHealthMonitoringCommand"/>.</summary>
    public BeginCorrectiveActionEnvironmentalHealthMonitoringCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EnvironmentalHealthMonitoringId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
    }
}