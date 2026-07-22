// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ExecuteInspectionMachineGuardingInspectionsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.ExecuteInspectionMachineGuardingInspectionsCommand"/>.</summary>
public sealed class ExecuteInspectionMachineGuardingInspectionsCommandValidator : AbstractValidator<Commands.ExecuteInspectionMachineGuardingInspectionsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ExecuteInspectionMachineGuardingInspectionsCommand"/>.</summary>
    public ExecuteInspectionMachineGuardingInspectionsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineGuardingInspectionsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.DeficiencyCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ComplianceScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.InspectedByUserId).NotEmpty();
    }
}