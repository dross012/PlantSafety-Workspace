// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleInspectionMachineGuardingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleInspectionMachineGuardingCommand"/>.</summary>
public sealed class ScheduleInspectionMachineGuardingCommandValidator : AbstractValidator<Commands.ScheduleInspectionMachineGuardingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleInspectionMachineGuardingCommand"/>.</summary>
    public ScheduleInspectionMachineGuardingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}