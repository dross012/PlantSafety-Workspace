// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: FluentValidation validator for QualifyOperatorCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.QualifyOperatorCommand"/>.</summary>
public sealed class QualifyOperatorCommandValidator : AbstractValidator<Commands.QualifyOperatorCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.QualifyOperatorCommand"/>.</summary>
    public QualifyOperatorCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CraneHoistSystemsId).NotEmpty();
        RuleFor(x => x.OperatorUserId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
