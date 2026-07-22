// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for BeginRepairSafetyValvesRelaysCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.BeginRepairSafetyValvesRelaysCommand"/>.</summary>
public sealed class BeginRepairSafetyValvesRelaysCommandValidator : AbstractValidator<Commands.BeginRepairSafetyValvesRelaysCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.BeginRepairSafetyValvesRelaysCommand"/>.</summary>
    public BeginRepairSafetyValvesRelaysCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyValvesRelaysId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
    }
}