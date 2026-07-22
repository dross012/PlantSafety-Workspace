// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ImplementControlsErgonomicsAndMaterialHandlingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.ImplementControlsErgonomicsAndMaterialHandlingCommand"/>.</summary>
public sealed class ImplementControlsErgonomicsAndMaterialHandlingCommandValidator : AbstractValidator<Commands.ImplementControlsErgonomicsAndMaterialHandlingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ImplementControlsErgonomicsAndMaterialHandlingCommand"/>.</summary>
    public ImplementControlsErgonomicsAndMaterialHandlingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ErgonomicsAndMaterialHandlingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ControlsDescription).NotEmpty().MaximumLength(1000);
    }
}