// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for EscalatePMSchedulingCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Scheduling.Validation;

/// <summary>Validates <see cref="Commands.EscalatePMSchedulingCommand"/>.</summary>
public sealed class EscalatePMSchedulingCommandValidator : AbstractValidator<Commands.EscalatePMSchedulingCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.EscalatePMSchedulingCommand"/>.</summary>
    public EscalatePMSchedulingCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PMSchedulingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.PriorityCode).NotEmpty().Must(p => p is "P1" or "P2" or "P3" or "P4")
            .WithMessage("PriorityCode must be one of P1, P2, P3, P4.");
    }
}
