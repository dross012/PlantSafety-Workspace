// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: FluentValidation validator for CreateSafetyProgramCommand. Enforces
//   invariants and normalization boundaries only; never calls repositories or external
//   services (Handbook Rule 2.3).
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.SafetyPrograms.Validation;

/// <summary>Validates <see cref="Commands.CreateSafetyProgramCommand"/>.</summary>
public sealed class CreateSafetyProgramCommandValidator : AbstractValidator<Commands.CreateSafetyProgramCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.CreateSafetyProgramCommand"/>.</summary>
    public CreateSafetyProgramCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.EndDate)
            .Must((command, endDate) => endDate is null || endDate >= command.StartDate)
            .WithMessage("EndDate must not precede StartDate.");
    }
}
