// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for RecordRiggingInspectionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordRiggingInspectionCommand"/>.</summary>
public sealed class RecordRiggingInspectionCommandValidator : AbstractValidator<Commands.RecordRiggingInspectionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordRiggingInspectionCommand"/>.</summary>
    public RecordRiggingInspectionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CraneHoistSystemsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
