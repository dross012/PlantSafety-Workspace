// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ApplyIsolationLotoSystemsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.ApplyIsolationLotoSystemsCommand"/>.</summary>
public sealed class ApplyIsolationLotoSystemsCommandValidator : AbstractValidator<Commands.ApplyIsolationLotoSystemsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ApplyIsolationLotoSystemsCommand"/>.</summary>
    public ApplyIsolationLotoSystemsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoSystemsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}