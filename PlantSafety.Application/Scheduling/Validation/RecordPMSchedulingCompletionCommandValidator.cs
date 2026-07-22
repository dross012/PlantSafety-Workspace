// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordPMSchedulingCompletionCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.Scheduling.Validation;

/// <summary>Validates <see cref="Commands.RecordPMSchedulingCompletionCommand"/>.</summary>
public sealed class RecordPMSchedulingCompletionCommandValidator : AbstractValidator<Commands.RecordPMSchedulingCompletionCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordPMSchedulingCompletionCommand"/>.</summary>
    public RecordPMSchedulingCompletionCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PMSchedulingId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.CompletionNotes).MaximumLength(2000);
    }
}
