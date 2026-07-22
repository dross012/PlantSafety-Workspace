// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordPPEFitTestCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PPE.Validation;

/// <summary>Validates <see cref="Commands.RecordPPEFitTestCommand"/>.</summary>
public sealed class RecordPPEFitTestCommandValidator : AbstractValidator<Commands.RecordPPEFitTestCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordPPEFitTestCommand"/>.</summary>
    public RecordPPEFitTestCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PPEAssignmentId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
