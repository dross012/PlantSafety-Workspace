// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for FlagBypassMachineGuardingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.FlagBypassMachineGuardingCommand"/>.</summary>
public sealed class FlagBypassMachineGuardingCommandValidator : AbstractValidator<Commands.FlagBypassMachineGuardingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagBypassMachineGuardingCommand"/>.</summary>
    public FlagBypassMachineGuardingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.FindingNotes).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.DetectedByUserId).NotEmpty();
    }
}