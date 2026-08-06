// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: FluentValidation validator for MarkCorrectiveActionOverdueCommand.
// SchemaVersion: 1

using FluentValidation;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.MarkOverdue;
using PlantSafety.Application.Validation.Extensions;

namespace PlantSafety.Application.Features.ContinuousImprovement.Validators.CAPA;

/// <summary>Validates <see cref="MarkCorrectiveActionOverdueCommand"/> structural fields.</summary>
public sealed class MarkCorrectiveActionOverdueCommandValidator : AbstractValidator<MarkCorrectiveActionOverdueCommand>
{
    /// <summary>Defines validation rules.</summary>
    public MarkCorrectiveActionOverdueCommandValidator()
    {
        RuleFor(x => x.CommandId).IsValidId("CommandId");
        RuleFor(x => x.CorrelationId).IsValidId("CorrelationId");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.CaId).NotEmpty();
    }
}
