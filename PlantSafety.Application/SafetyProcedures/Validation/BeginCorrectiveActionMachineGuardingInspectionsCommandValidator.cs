// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for BeginCorrectiveActionMachineGuardingInspectionsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.BeginCorrectiveActionMachineGuardingInspectionsCommand"/>.</summary>
public sealed class BeginCorrectiveActionMachineGuardingInspectionsCommandValidator : AbstractValidator<Commands.BeginCorrectiveActionMachineGuardingInspectionsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginCorrectiveActionMachineGuardingInspectionsCommand"/>.</summary>
    public BeginCorrectiveActionMachineGuardingInspectionsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardingInspectionsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}