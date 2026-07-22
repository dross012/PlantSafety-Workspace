// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RaiseMachineAnomalyFlagCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Observability.Validation;

/// <summary>Validates <see cref="Commands.RaiseMachineAnomalyFlagCommand"/>.</summary>
public sealed class RaiseMachineAnomalyFlagCommandValidator : AbstractValidator<Commands.RaiseMachineAnomalyFlagCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RaiseMachineAnomalyFlagCommand"/>.</summary>
    public RaiseMachineAnomalyFlagCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MachineProcessMonitoringId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.AnomalyDescription).NotEmpty().MaximumLength(1000);
    }
}
