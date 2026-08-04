// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for CompleteCraneInspectionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.CompleteCraneInspectionCommand"/>.</summary>
public sealed class CompleteCraneInspectionCommandValidator : AbstractValidator<Commands.CompleteCraneInspectionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CompleteCraneInspectionCommand"/>.</summary>
    public CompleteCraneInspectionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CraneHoistSystemsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
