// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: FluentValidation validator for RecordPermitClosureSignatureCommand.
// SchemaVersion: 1

using FluentValidation;

namespace PlantSafety.Application.PermitToWork.Validation;

/// <summary>Validates <see cref="Commands.RecordPermitClosureSignatureCommand"/>.</summary>
public sealed class RecordPermitClosureSignatureCommandValidator : AbstractValidator<Commands.RecordPermitClosureSignatureCommand>
{
    /// <summary>Defines validation rules for <see cref="Commands.RecordPermitClosureSignatureCommand"/>.</summary>
    public RecordPermitClosureSignatureCommandValidator()
    {
        RuleFor(x => x.CommandId).NotEmpty();
        RuleFor(x => x.CorrelationId).NotEmpty();
        RuleFor(x => x.CausationId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PermitWorkflowId).NotEmpty();
        RuleFor(x => x.ClosureSignedByUserId).NotEmpty();
        RuleFor(x => x.ClosureNotes).MaximumLength(2000);
    }
}
