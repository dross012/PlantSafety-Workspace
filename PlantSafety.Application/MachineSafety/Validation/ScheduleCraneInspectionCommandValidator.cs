// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for ScheduleCraneInspectionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.ScheduleCraneInspectionCommand"/>.</summary>
public sealed class ScheduleCraneInspectionCommandValidator : AbstractValidator<Commands.ScheduleCraneInspectionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ScheduleCraneInspectionCommand"/>.</summary>
    public ScheduleCraneInspectionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CraneHoistSystemsId).NotEmpty();
        RuleFor(x => x.DueAtUtc).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
