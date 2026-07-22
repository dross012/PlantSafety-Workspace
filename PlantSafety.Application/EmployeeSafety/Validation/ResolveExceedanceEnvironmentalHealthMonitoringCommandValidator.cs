// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ResolveExceedanceEnvironmentalHealthMonitoringCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.ResolveExceedanceEnvironmentalHealthMonitoringCommand"/>.</summary>
public sealed class ResolveExceedanceEnvironmentalHealthMonitoringCommandValidator : AbstractValidator<Commands.ResolveExceedanceEnvironmentalHealthMonitoringCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ResolveExceedanceEnvironmentalHealthMonitoringCommand"/>.</summary>
    public ResolveExceedanceEnvironmentalHealthMonitoringCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EnvironmentalHealthMonitoringId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ReMeasuredScore).InclusiveBetween(0m, 100m);
    }
}