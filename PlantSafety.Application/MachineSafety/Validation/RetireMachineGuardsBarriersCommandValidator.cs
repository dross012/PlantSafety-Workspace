// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireMachineGuardsBarriersCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RetireMachineGuardsBarriersCommand"/>.</summary>
public sealed class RetireMachineGuardsBarriersCommandValidator : AbstractValidator<Commands.RetireMachineGuardsBarriersCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireMachineGuardsBarriersCommand"/>.</summary>
    public RetireMachineGuardsBarriersCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardsBarriersId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}