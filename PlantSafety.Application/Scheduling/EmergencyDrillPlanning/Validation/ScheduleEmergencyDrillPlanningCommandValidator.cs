// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           ScheduleEmergencyDrillPlanningCommandValidator.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ScheduleEmergencyDrillPlanningCommand. Enforces
//                  invariants and normalization boundaries only; never calls repositories or
//                  external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Validation;

/// <summary>Validates <see cref="Commands.ScheduleEmergencyDrillPlanningCommand"/>.</summary>
public sealed class ScheduleEmergencyDrillPlanningCommandValidator : AbstractValidator<Commands.ScheduleEmergencyDrillPlanningCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleEmergencyDrillPlanningCommand"/>.</summary>
    public ScheduleEmergencyDrillPlanningCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ScheduledByUserId).NotEmpty();
        RuleFor(x => x.DrillType).NotEmpty().MaximumLength(400);
        RuleFor(x => x.ShiftCode).NotEmpty().MaximumLength(400);
        RuleFor(x => x.ParticipationCount).GreaterThanOrEqualTo(0);
    }
}
