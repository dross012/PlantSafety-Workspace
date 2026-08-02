// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           RecordShiftFatigueCommandValidator.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for RecordShiftFatigueCommand. Enforces invariants
//                  and normalization boundaries only; never calls repositories or external
//                  services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Validation;

/// <summary>Validates <see cref="Commands.RecordShiftFatigueCommand"/>.</summary>
public sealed class RecordShiftFatigueCommandValidator : AbstractValidator<Commands.RecordShiftFatigueCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordShiftFatigueCommand"/>.</summary>
    public RecordShiftFatigueCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ShiftPattern).NotEmpty().MaximumLength(400);
        RuleFor(x => x.ConsecutiveNightShifts).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeHours).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.FatigueRiskScore).GreaterThanOrEqualTo(0m);
    }
}
