// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for VerifyCorrectiveActionMachineGuardingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.VerifyCorrectiveActionMachineGuardingCommand"/>.</summary>
public sealed class VerifyCorrectiveActionMachineGuardingCommandValidator : AbstractValidator<Commands.VerifyCorrectiveActionMachineGuardingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.VerifyCorrectiveActionMachineGuardingCommand"/>.</summary>
    public VerifyCorrectiveActionMachineGuardingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.VerifiedByUserId).NotEmpty();
    }
}