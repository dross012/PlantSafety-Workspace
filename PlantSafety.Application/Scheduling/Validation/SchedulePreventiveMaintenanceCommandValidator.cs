// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for SchedulePreventiveMaintenanceCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Scheduling.Validation;

/// <summary>Validates <see cref="Commands.SchedulePreventiveMaintenanceCommand"/>.</summary>
public sealed class SchedulePreventiveMaintenanceCommandValidator : AbstractValidator<Commands.SchedulePreventiveMaintenanceCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SchedulePreventiveMaintenanceCommand"/>.</summary>
    public SchedulePreventiveMaintenanceCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PMSchedulingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.DueAtUtc).NotEmpty();
    }
}
