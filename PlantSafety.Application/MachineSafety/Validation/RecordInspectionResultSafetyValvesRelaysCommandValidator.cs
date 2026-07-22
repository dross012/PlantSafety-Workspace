// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordInspectionResultSafetyValvesRelaysCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.MachineSafety.Validation;

/// <summary>Validates <see cref="Commands.RecordInspectionResultSafetyValvesRelaysCommand"/>.</summary>
public sealed class RecordInspectionResultSafetyValvesRelaysCommandValidator : AbstractValidator<Commands.RecordInspectionResultSafetyValvesRelaysCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordInspectionResultSafetyValvesRelaysCommand"/>.</summary>
    public RecordInspectionResultSafetyValvesRelaysCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SafetyValvesRelaysId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ReliabilityScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.InspectedByUserId).NotEmpty();
    }
}