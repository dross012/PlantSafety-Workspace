// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordInspectionResultMachineGuardingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordInspectionResultMachineGuardingCommand"/>.</summary>
public sealed class RecordInspectionResultMachineGuardingCommandValidator : AbstractValidator<Commands.RecordInspectionResultMachineGuardingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordInspectionResultMachineGuardingCommand"/>.</summary>
    public RecordInspectionResultMachineGuardingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.DeficiencyCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ConditionScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.InspectedByUserId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}