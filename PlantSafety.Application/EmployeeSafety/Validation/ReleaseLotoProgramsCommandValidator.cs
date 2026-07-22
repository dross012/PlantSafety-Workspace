// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ReleaseLotoProgramsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.ReleaseLotoProgramsCommand"/>.</summary>
public sealed class ReleaseLotoProgramsCommandValidator : AbstractValidator<Commands.ReleaseLotoProgramsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ReleaseLotoProgramsCommand"/>.</summary>
    public ReleaseLotoProgramsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProgramsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ReleasedByUserId).NotEmpty();
    }
}