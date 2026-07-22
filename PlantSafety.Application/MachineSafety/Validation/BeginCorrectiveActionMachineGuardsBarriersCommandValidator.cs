// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for BeginCorrectiveActionMachineGuardsBarriersCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.BeginCorrectiveActionMachineGuardsBarriersCommand"/>.</summary>
public sealed class BeginCorrectiveActionMachineGuardsBarriersCommandValidator : AbstractValidator<Commands.BeginCorrectiveActionMachineGuardsBarriersCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginCorrectiveActionMachineGuardsBarriersCommand"/>.</summary>
    public BeginCorrectiveActionMachineGuardsBarriersCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardsBarriersId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}