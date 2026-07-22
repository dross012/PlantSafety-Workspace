// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RetireSafetyValvesRelaysCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RetireSafetyValvesRelaysCommand"/>.</summary>
public sealed class RetireSafetyValvesRelaysCommandValidator : AbstractValidator<Commands.RetireSafetyValvesRelaysCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RetireSafetyValvesRelaysCommand"/>.</summary>
    public RetireSafetyValvesRelaysCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyValvesRelaysId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}