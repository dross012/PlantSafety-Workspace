// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: FluentValidation validator for ChangeSafetyProgramStatusCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyPrograms.Validation;

/// <summary>Validates <see cref="Commands.ChangeSafetyProgramStatusCommand"/>.</summary>
public sealed class ChangeSafetyProgramStatusCommandValidator : AbstractValidator<Commands.ChangeSafetyProgramStatusCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ChangeSafetyProgramStatusCommand"/>.</summary>
    public ChangeSafetyProgramStatusCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyProgramId).NotEmpty();
        RuleFor(x => x.NewStatus).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}
