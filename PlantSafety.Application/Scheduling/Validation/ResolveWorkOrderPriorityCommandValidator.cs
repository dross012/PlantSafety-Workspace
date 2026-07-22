// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for ResolveWorkOrderPriorityCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Scheduling.Validation;

/// <summary>Validates <see cref="Commands.ResolveWorkOrderPriorityCommand"/>.</summary>
public sealed class ResolveWorkOrderPriorityCommandValidator : AbstractValidator<Commands.ResolveWorkOrderPriorityCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.ResolveWorkOrderPriorityCommand"/>.</summary>
    public ResolveWorkOrderPriorityCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.WorkOrderPriorityId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.ResolutionNotes).MaximumLength(2000);
    }
}
