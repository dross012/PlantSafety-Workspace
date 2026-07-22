// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ReleaseLotoSystemsCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.ReleaseLotoSystemsCommand"/>.</summary>
public sealed class ReleaseLotoSystemsCommandValidator : AbstractValidator<Commands.ReleaseLotoSystemsCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ReleaseLotoSystemsCommand"/>.</summary>
    public ReleaseLotoSystemsCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LotoSystemsId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}