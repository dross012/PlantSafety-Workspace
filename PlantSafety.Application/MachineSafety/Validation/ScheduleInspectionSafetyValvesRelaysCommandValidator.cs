// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleInspectionSafetyValvesRelaysCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleInspectionSafetyValvesRelaysCommand"/>.</summary>
public sealed class ScheduleInspectionSafetyValvesRelaysCommandValidator : AbstractValidator<Commands.ScheduleInspectionSafetyValvesRelaysCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleInspectionSafetyValvesRelaysCommand"/>.</summary>
    public ScheduleInspectionSafetyValvesRelaysCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyValvesRelaysId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}