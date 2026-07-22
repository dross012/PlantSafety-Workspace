// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordReadinessEmergencyPreparednessCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordReadinessEmergencyPreparednessCommand"/>.</summary>
public sealed class RecordReadinessEmergencyPreparednessCommandValidator : AbstractValidator<Commands.RecordReadinessEmergencyPreparednessCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordReadinessEmergencyPreparednessCommand"/>.</summary>
    public RecordReadinessEmergencyPreparednessCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmergencyPreparednessId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ReadinessScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.GapNotes).MaximumLength(1000);
    }
}