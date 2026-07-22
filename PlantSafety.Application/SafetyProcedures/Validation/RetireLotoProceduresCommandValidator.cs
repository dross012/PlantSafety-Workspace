// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireLotoProceduresCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyProcedures.Validation;

/// <summary>Validates <see cref="Commands.RetireLotoProceduresCommand"/>.</summary>
public sealed class RetireLotoProceduresCommandValidator : AbstractValidator<Commands.RetireLotoProceduresCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireLotoProceduresCommand"/>.</summary>
    public RetireLotoProceduresCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProceduresId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}