// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for EscalateWorkOrderPriorityCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Scheduling.Validation;

/// <summary>Validates <see cref="Commands.EscalateWorkOrderPriorityCommand"/>.</summary>
public sealed class EscalateWorkOrderPriorityCommandValidator : AbstractValidator<Commands.EscalateWorkOrderPriorityCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.EscalateWorkOrderPriorityCommand"/>.</summary>
    public EscalateWorkOrderPriorityCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WorkOrderPriorityId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.EscalationReason).NotEmpty().MaximumLength(1000);
    }
}
