// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for VerifyCorrectiveActionMachineGuardsBarriersCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.VerifyCorrectiveActionMachineGuardsBarriersCommand"/>.</summary>
public sealed class VerifyCorrectiveActionMachineGuardsBarriersCommandValidator : AbstractValidator<Commands.VerifyCorrectiveActionMachineGuardsBarriersCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.VerifyCorrectiveActionMachineGuardsBarriersCommand"/>.</summary>
    public VerifyCorrectiveActionMachineGuardsBarriersCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardsBarriersId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}