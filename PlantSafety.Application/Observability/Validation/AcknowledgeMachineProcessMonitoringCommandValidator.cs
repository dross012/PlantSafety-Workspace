// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for AcknowledgeMachineProcessMonitoringCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Observability.Validation;

/// <summary>Validates <see cref="Commands.AcknowledgeMachineProcessMonitoringCommand"/>.</summary>
public sealed class AcknowledgeMachineProcessMonitoringCommandValidator : AbstractValidator<Commands.AcknowledgeMachineProcessMonitoringCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.AcknowledgeMachineProcessMonitoringCommand"/>.</summary>
    public AcknowledgeMachineProcessMonitoringCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineProcessMonitoringId).NotEmpty();
        RuleFor(x => x.AcknowledgedByUserId).NotEmpty();
    }
}
