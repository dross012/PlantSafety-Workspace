// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ConductDrillEmergencyPreparednessCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.ConductDrillEmergencyPreparednessCommand"/>.</summary>
public sealed class ConductDrillEmergencyPreparednessCommandValidator : AbstractValidator<Commands.ConductDrillEmergencyPreparednessCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ConductDrillEmergencyPreparednessCommand"/>.</summary>
    public ConductDrillEmergencyPreparednessCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyPreparednessId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ParticipantCount).GreaterThanOrEqualTo(0);
    }
}