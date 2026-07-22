// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetirePpeCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.RetirePpeCommand"/>.</summary>
public sealed class RetirePpeCommandValidator : AbstractValidator<Commands.RetirePpeCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetirePpeCommand"/>.</summary>
    public RetirePpeCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PpeId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}