// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for FlagForReplacementPpeCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.FlagForReplacementPpeCommand"/>.</summary>
public sealed class FlagForReplacementPpeCommandValidator : AbstractValidator<Commands.FlagForReplacementPpeCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.FlagForReplacementPpeCommand"/>.</summary>
    public FlagForReplacementPpeCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PpeId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}