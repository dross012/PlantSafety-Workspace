// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordMachineSensorReadingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Observability.Validation;

/// <summary>Validates <see cref="Commands.RecordMachineSensorReadingCommand"/>.</summary>
public sealed class RecordMachineSensorReadingCommandValidator : AbstractValidator<Commands.RecordMachineSensorReadingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordMachineSensorReadingCommand"/>.</summary>
    public RecordMachineSensorReadingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineProcessMonitoringId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ThresholdMax).GreaterThanOrEqualTo(x => x.ThresholdMin);
    }
}
