// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           SchedulePreventiveMaintenanceTaskCommandValidator.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for SchedulePreventiveMaintenanceTaskCommand.
//                  Enforces invariants and normalization boundaries only; never calls
//                  repositories or external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Validation;

/// <summary>Validates <see cref="Commands.SchedulePreventiveMaintenanceTaskCommand"/>.</summary>
public sealed class SchedulePreventiveMaintenanceTaskCommandValidator : AbstractValidator<Commands.SchedulePreventiveMaintenanceTaskCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.SchedulePreventiveMaintenanceTaskCommand"/>.</summary>
    public SchedulePreventiveMaintenanceTaskCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.TaskDescription).NotEmpty().MaximumLength(400);
        RuleFor(x => x.ScheduledAtUtc).NotEqual(default(DateTime));
    }
}
