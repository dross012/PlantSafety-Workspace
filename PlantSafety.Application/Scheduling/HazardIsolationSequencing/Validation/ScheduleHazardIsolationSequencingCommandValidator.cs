// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           ScheduleHazardIsolationSequencingCommandValidator.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    FluentValidation validator for ScheduleHazardIsolationSequencingCommand.
//                  Enforces invariants and normalization boundaries only; never calls repositories
//                  or external services (Handbook Rule 2.3). Mirrors
//                  PlantSafety.Application.Compliance.Permits.Validation.CreatePermitsCommandValidator.
//                  Additive-only evolution.
// ============================================================================
using FluentValidation;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Validation;

/// <summary>Validates <see cref="Commands.ScheduleHazardIsolationSequencingCommand"/>.</summary>
public sealed class ScheduleHazardIsolationSequencingCommandValidator : AbstractValidator<Commands.ScheduleHazardIsolationSequencingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleHazardIsolationSequencingCommand"/>.</summary>
    public ScheduleHazardIsolationSequencingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RequesterUserId).NotEmpty();
        RuleFor(x => x.HazardZone).NotEmpty().MaximumLength(400);
        RuleFor(x => x.TaskType).NotEmpty().MaximumLength(400);
        RuleFor(x => x.WindowEndUtc)
            .GreaterThanOrEqualTo(x => x.WindowStartUtc!.Value)
            .When(x => x.WindowStartUtc.HasValue && x.WindowEndUtc.HasValue)
            .WithMessage("WindowEndUtc must not be earlier than WindowStartUtc.");
    }
}
