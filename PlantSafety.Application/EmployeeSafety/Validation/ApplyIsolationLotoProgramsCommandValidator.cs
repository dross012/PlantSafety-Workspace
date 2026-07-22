// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ApplyIsolationLotoProgramsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.EmployeeSafety.Validation;

/// <summary>Validates <see cref="Commands.ApplyIsolationLotoProgramsCommand"/>.</summary>
public sealed class ApplyIsolationLotoProgramsCommandValidator : AbstractValidator<Commands.ApplyIsolationLotoProgramsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ApplyIsolationLotoProgramsCommand"/>.</summary>
    public ApplyIsolationLotoProgramsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoProgramsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.LocksAppliedCount).GreaterThanOrEqualTo(0);
    }
}