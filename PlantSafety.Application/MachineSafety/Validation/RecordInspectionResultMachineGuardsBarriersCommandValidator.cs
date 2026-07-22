// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordInspectionResultMachineGuardsBarriersCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordInspectionResultMachineGuardsBarriersCommand"/>.</summary>
public sealed class RecordInspectionResultMachineGuardsBarriersCommandValidator : AbstractValidator<Commands.RecordInspectionResultMachineGuardsBarriersCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordInspectionResultMachineGuardsBarriersCommand"/>.</summary>
    public RecordInspectionResultMachineGuardsBarriersCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardsBarriersId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.DeficiencyCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GuardIntegrityScore).InclusiveBetween(0m, 100m);
    }
}