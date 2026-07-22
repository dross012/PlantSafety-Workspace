// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ScheduleDrillEmergencyPreparednessCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleDrillEmergencyPreparednessCommand"/>.</summary>
public sealed class ScheduleDrillEmergencyPreparednessCommandValidator : AbstractValidator<Commands.ScheduleDrillEmergencyPreparednessCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleDrillEmergencyPreparednessCommand"/>.</summary>
    public ScheduleDrillEmergencyPreparednessCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyPreparednessId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}