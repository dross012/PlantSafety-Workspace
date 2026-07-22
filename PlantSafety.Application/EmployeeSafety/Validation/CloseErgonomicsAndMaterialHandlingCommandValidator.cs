// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for CloseErgonomicsAndMaterialHandlingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.CloseErgonomicsAndMaterialHandlingCommand"/>.</summary>
public sealed class CloseErgonomicsAndMaterialHandlingCommandValidator : AbstractValidator<Commands.CloseErgonomicsAndMaterialHandlingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CloseErgonomicsAndMaterialHandlingCommand"/>.</summary>
    public CloseErgonomicsAndMaterialHandlingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ErgonomicsAndMaterialHandlingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}